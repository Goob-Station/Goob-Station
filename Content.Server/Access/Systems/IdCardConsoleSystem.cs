// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Containers;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Components;
using static Content.Shared.Access.Components.IdCardConsoleComponent;
using Content.Shared.Access.Systems;
using Content.Shared.Access;
using Content.Shared.Administration.Logs;
using Content.Shared.Chat;
using Content.Shared.Construction;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Server.Station.Systems; // Pirate: records photos
using Content.Shared.CriminalRecords; // Pirate: records photos
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
// Pirate banking start
using Content.Server._Pirate.Banking;
using Content.Shared._Pirate.Banking;
using Content.Server.Roles.Jobs;
using Content.Server.Roles;
// Pirate banking end

namespace Content.Server.Access.Systems;

[UsedImplicitly]
public sealed class IdCardConsoleSystem : SharedIdCardConsoleSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly StationRecordsSystem _record = default!;
    #region Pirate: records photos
    [Dependency] private readonly StationSystem _station = default!;
    #endregion
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly AccessSystem _access = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    // Pirate banking start
    [Dependency] private readonly RoleSystem _roles = default!;
    [Dependency] private readonly JobSystem _job = default!;
    [Dependency] private readonly BankCardSystem _bankCard = default!;
    // Pirate banking end

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdCardConsoleComponent, WriteToTargetIdMessage>(OnWriteToTargetIdMessage);
        SubscribeLocalEvent<IdCardConsoleComponent, SetTargetIdAccessMessage>(OnSetTargetIdAccessMessage); // Pirate: id card console fix

        // one day, maybe bound user interfaces can be shared too.
        SubscribeLocalEvent<IdCardConsoleComponent, ComponentStartup>(UpdateUserInterface);
        SubscribeLocalEvent<IdCardConsoleComponent, EntInsertedIntoContainerMessage>(UpdateUserInterface);
        SubscribeLocalEvent<IdCardConsoleComponent, EntRemovedFromContainerMessage>(UpdateUserInterface);
        SubscribeLocalEvent<IdCardConsoleComponent, DamageChangedEvent>(OnDamageChanged);

        // Intercept the event before anyone can do anything with it!
        SubscribeLocalEvent<IdCardConsoleComponent, MachineDeconstructedEvent>(OnMachineDeconstructed,
            before: [typeof(EmptyOnMachineDeconstructSystem), typeof(ItemSlotsSystem)]);
    }

    private void OnWriteToTargetIdMessage(EntityUid uid, IdCardConsoleComponent component, WriteToTargetIdMessage args)
    {
        if (args.Actor is not { Valid: true } player)
            return;

        TryWriteToTargetId(uid, args.FullName, args.JobTitle, args.AccessList, args.JobPrototype, player, component);

        UpdateUserInterface(uid, component, args);
    }

    private void UpdateUserInterface(EntityUid uid, IdCardConsoleComponent component, EntityEventArgs args)
    {
        if (!component.Initialized)
            return;

        var privilegedIdName = string.Empty;
        List<ProtoId<AccessLevelPrototype>>? possibleAccess = null;
        if (component.PrivilegedIdSlot.Item is { Valid: true } item)
        {
            privilegedIdName = Comp<MetaDataComponent>(item).EntityName;
            possibleAccess = _accessReader.FindAccessTags(item).ToList();
        }

        IdCardConsoleBoundUserInterfaceState newState;
        // this could be prettier
        if (component.TargetIdSlot.Item is not { Valid: true } targetId)
        {
            newState = new IdCardConsoleBoundUserInterfaceState(
                component.PrivilegedIdSlot.HasItem,
                PrivilegedIdIsAuthorized(uid, component, out _),
                false,
                null,
                null,
                null,
                possibleAccess,
                string.Empty,
                privilegedIdName,
                string.Empty);
        }
        else
        {
            var targetIdComponent = Comp<IdCardComponent>(targetId);
            var targetAccessComponent = Comp<AccessComponent>(targetId);

            var jobProto = targetIdComponent.JobPrototype ?? new ProtoId<JobPrototype>(string.Empty);
            if (TryComp<StationRecordKeyStorageComponent>(targetId, out var keyStorage)
                && keyStorage.Key is { } key
                && _record.TryGetRecord<GeneralStationRecord>(key, out var record))
            {
                jobProto = record.JobPrototype;
            }

            newState = new IdCardConsoleBoundUserInterfaceState(
                component.PrivilegedIdSlot.HasItem,
                PrivilegedIdIsAuthorized(uid, component, out _),
                true,
                targetIdComponent.FullName,
                targetIdComponent.LocalizedJobTitle,
                targetAccessComponent.Tags.ToList(),
                possibleAccess,
                jobProto,
                privilegedIdName,
                Name(targetId));
        }

        _userInterface.SetUiState(uid, IdCardConsoleUiKey.Key, newState);
    }

    /// <summary>
    /// Called whenever an access button is pressed, adding or removing that access from the target ID card.
    /// Writes data passed from the UI into the ID stored in <see cref="IdCardConsoleComponent.TargetIdSlot"/>, if present.
    /// </summary>
    private void TryWriteToTargetId(EntityUid uid,
        string newFullName,
        string newJobTitle,
        List<ProtoId<AccessLevelPrototype>> newAccessList,
        ProtoId<JobPrototype> newJobProto,
        EntityUid player,
        IdCardConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.TargetIdSlot.Item is not { Valid: true } targetId || !PrivilegedIdIsAuthorized(uid, component, out var privilegedId))
            return;

        _idCard.TryChangeFullName(targetId, newFullName, player: player);
        _idCard.TryChangeJobTitle(targetId, newJobTitle, player: player);

        if (_prototype.Resolve(newJobProto, out var job)
            && _prototype.Resolve(job.Icon, out var jobIcon))
        {
            _idCard.TryChangeJobIcon(targetId, jobIcon, player: player);
            _idCard.TryChangeJobDepartment(targetId, job);
            // Pirate START - Fix alt jobs screwing allowed jobs check
            if (TryComp<IdCardComponent>(targetId, out var idCardComp))
            {
                idCardComp.JobPrototype = job.ID;
            }
            // Pirate END - Fix alt jobs screwing allowed jobs check
        }

        #region Pirate: records photos
        TryLinkTargetIdToExistingRecord(uid, targetId, newFullName);
        #endregion
        UpdateStationRecord(uid, targetId, newFullName, newJobTitle, job);
        if ((!TryComp<StationRecordKeyStorageComponent>(targetId, out var keyStorage)
            || keyStorage.Key is not { } key
            || !_record.TryGetRecord<GeneralStationRecord>(key, out _))
            && newJobProto != string.Empty)
        {
            Comp<IdCardComponent>(targetId).JobPrototype = newJobProto;
        }

        if (!newAccessList.TrueForAll(x => component.AccessLevels.Contains(x)))
        {
            _sawmill.Warning($"User {ToPrettyString(uid)} tried to write unknown access tag.");
            return;
        }

        var oldTags = _access.TryGetTags(targetId)?.ToList() ?? new List<ProtoId<AccessLevelPrototype>>();

        if (oldTags.SequenceEqual(newAccessList))
            return;

        // I hate that C# doesn't have an option for this and don't desire to write this out the hard way.
        // var difference = newAccessList.Difference(oldTags);
        var difference = newAccessList.Union(oldTags).Except(newAccessList.Intersect(oldTags)).ToHashSet();
        var privilegedPerms = _accessReader.FindAccessTags(privilegedId.Value);
        if (!difference.IsSubsetOf(privilegedPerms))
        {
            _sawmill.Warning($"User {ToPrettyString(uid)} tried to modify permissions they could not give/take!");
            return;
        }

        var addedTags = newAccessList.Except(oldTags).Select(tag => "+" + tag).ToList();
        var removedTags = oldTags.Except(newAccessList).Select(tag => "-" + tag).ToList();
        _access.TrySetTags(targetId, newAccessList);

        // Pirate banking start
        if (newJobProto.Id != string.Empty &&
            TryComp<BankCardComponent>(targetId, out var bankCard) &&
            bankCard.AccountId.HasValue &&
            _bankCard.TryGetAccount(bankCard.AccountId.Value, out var account) &&
            account.Mind != null)
        {
            var mindId = GetEntity(account.Mind.Value);

            if (_prototype.TryIndex<JobPrototype>(newJobProto, out var jobPrototype))
            {
                if (_job.MindTryGetJob(mindId, out var oldJob))
                {
                    _roles.MindRemoveRole(mindId, oldJob.ID);
                }
                _job.MindAddJob(mindId, jobPrototype.ID);
            }
        }
        // Pirate banking end

        /*TODO: ECS SharedIdCardConsoleComponent and then log on card ejection, together with the save.
        This current implementation is pretty shit as it logs 27 entries (27 lines) if someone decides to give themselves AA*/
        _adminLogger.Add(LogType.Action,
            $"{player} has modified {targetId} with the following accesses: [{string.Join(", ", addedTags.Union(removedTags))}] [{string.Join(", ", newAccessList)}]");
    }

    /// <summary>
    /// Returns true if there is an ID in <see cref="IdCardConsoleComponent.PrivilegedIdSlot"/> and said ID satisfies the requirements of <see cref="AccessReaderComponent"/>.
    /// </summary>
    private bool PrivilegedIdIsAuthorized(EntityUid uid, IdCardConsoleComponent component, [NotNullWhen(true)] out EntityUid? id)
    {
        id = null;
        if (component.PrivilegedIdSlot.Item == null)
            return false;

        id = component.PrivilegedIdSlot.Item;
        if (!TryComp<AccessReaderComponent>(uid, out var reader))
            return true;

        return _accessReader.IsAllowed(id.Value, uid, reader);
    }

    private void UpdateStationRecord(EntityUid uid, EntityUid targetId, string newFullName, ProtoId<AccessLevelPrototype> newJobTitle, JobPrototype? newJobProto)
    {
        if (!TryComp<StationRecordKeyStorageComponent>(targetId, out var keyStorage)
            || keyStorage.Key is not { } key
            || !_record.TryGetRecord<GeneralStationRecord>(key, out var record))
        {
            return;
        }

        record.Name = newFullName;
        record.JobTitle = newJobTitle;

        if (newJobProto != null)
        {
            record.JobPrototype = newJobProto.ID;
            record.JobIcon = newJobProto.Icon;
        }

        #region Pirate: records photos
        if (_record.TryGetRecord<CriminalRecord>(key, out var criminalRecord))
        {
            criminalRecord.GeneralRecordSnapshot = record with { };

            if (criminalRecord.PortraitProfileSnapshot != null)
                criminalRecord.PortraitProfileSnapshot = criminalRecord.PortraitProfileSnapshot.WithName(newFullName);
        }
        #endregion

        _record.Synchronize(key);
    }

    #region Pirate: records photos
    private void TryLinkTargetIdToExistingRecord(EntityUid console, EntityUid targetId, string newFullName)
    {
        if (TryComp<StationRecordKeyStorageComponent>(targetId, out var keyStorage)
            && keyStorage.Key is { } existingKey
            && _record.TryGetRecord<GeneralStationRecord>(existingKey, out _))
        {
            return;
        }

        if (_station.GetOwningStation(console) is not { } station)
            return;

        var matchingIds = _record.GetRecordIdsByName(station, newFullName);
        if (matchingIds.Count != 1)
            return;

        var key = new StationRecordKey(matchingIds[0], station);
        if (!_record.TryGetRecord<GeneralStationRecord>(key, out _))
            return;

        _record.SetIdKey(targetId, key);
    }
    #endregion

    private void OnMachineDeconstructed(Entity<IdCardConsoleComponent> entity, ref MachineDeconstructedEvent args)
    {
        TryDropAndThrowIds(entity.AsNullable());
    }

    private void OnDamageChanged(Entity<IdCardConsoleComponent> entity, ref DamageChangedEvent args)
    {
        if (TryDropAndThrowIds(entity.AsNullable()))
            _chat.TrySendInGameICMessage(entity, Loc.GetString("id-card-console-damaged"), Shared.Chat.InGameICChatType.Speak, true);
    }

    #region PublicAPI

    /// <summary>
    ///     Tries to drop any IDs stored in the console, and then tries to throw them away.
    ///     Returns true if anything was ejected and false otherwise.
    /// </summary>
    public bool TryDropAndThrowIds(Entity<IdCardConsoleComponent?, ItemSlotsComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp1, ref ent.Comp2))
            return false;

        var didEject = false;

        foreach (var slot in ent.Comp2.Slots.Values)
        {
            if (slot.Item == null || slot.ContainerSlot == null)
                continue;

            var item = slot.Item.Value;
            if (_container.Remove(item, slot.ContainerSlot))
            {
                _throwing.TryThrow(item, _random.NextVector2(), baseThrowSpeed: 5f);
                didEject = true;
            }
        }

        return didEject;
    }

    #endregion

    #region Pirate: id card console fix
    private void TrySetTargetIdAccess(EntityUid uid,
        ProtoId<AccessLevelPrototype> access,
        bool enabled,
        EntityUid player,
        IdCardConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.TargetIdSlot.Item is not { Valid: true } targetId || !PrivilegedIdIsAuthorized(uid, component, out var privilegedId))
            return;

        if (!component.AccessLevels.Contains(access))
        {
            _sawmill.Warning($"User {ToPrettyString(uid)} tried to write unknown access tag.");
            return;
        }

        var privilegedPerms = _accessReader.FindAccessTags(privilegedId!.Value).ToHashSet();
        if (!privilegedPerms.Contains(access))
        {
            _sawmill.Warning($"User {ToPrettyString(uid)} tried to modify permissions they could not give/take!");
            return;
        }

        var oldTags = (_access.TryGetTags(targetId) ?? new List<ProtoId<AccessLevelPrototype>>()).ToHashSet();
        if (oldTags.Contains(access) == enabled)
            return;

        if (enabled)
            oldTags.Add(access);
        else
            oldTags.Remove(access);

        _access.TrySetTags(targetId, oldTags.ToList());

        var delta = (enabled ? "+" : "-") + access;
        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"{ToPrettyString(player):player} has modified {ToPrettyString(targetId):entity} with the following accesses: [{delta}] [{string.Join(", ", oldTags)}]");
    }
    private void OnSetTargetIdAccessMessage(EntityUid uid, IdCardConsoleComponent component, SetTargetIdAccessMessage args)
    {
        if (args.Actor is not { Valid: true } player)
            return;

        TrySetTargetIdAccess(uid, args.Access, args.Enabled, player, component);
        UpdateUserInterface(uid, component, args);
    }
    #endregion
}
