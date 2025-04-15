// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 chavonadelal <156101927+chavonadelal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Access.Systems;
using Content.Server.Administration.Logs;
using Content.Server.CriminalRecords.Systems;
using Content.Server.Humanoid;
using Content.Shared.Clothing;
using Content.Shared.Database;
using Content.Shared.Hands;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Server.IdentityManagement;

/// <summary>
///     Responsible for updating the identity of an entity on init or clothing equip/unequip.
/// </summary>
public sealed class IdentitySystem : SharedIdentitySystem
{
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly CriminalRecordsConsoleSystem _criminalRecordsConsole = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!; // Goobstation - Update component state on component toggle

    private HashSet<EntityUid> _queuedIdentityUpdates = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IdentityComponent, DidEquipEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, DidEquipHandEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, DidUnequipEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, DidUnequipHandEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, WearerMaskToggledEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, EntityRenamedEvent>((uid, _, _) => QueueIdentityUpdate(uid));
        SubscribeLocalEvent<IdentityComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<IdentityBlockerComponent, ComponentInit>(BlockerUpdateIdentity); // Goobstation - Update component state on component toggle
        SubscribeLocalEvent<IdentityBlockerComponent, ComponentRemove>(BlockerUpdateIdentity); // Goobstation - Update component state on component toggle
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var ent in _queuedIdentityUpdates)
        {
            if (!TryComp<IdentityComponent>(ent, out var identity))
                continue;

            UpdateIdentityInfo(ent, identity);
        }

        _queuedIdentityUpdates.Clear();
    }

    // This is where the magic happens
    private void OnMapInit(EntityUid uid, IdentityComponent component, MapInitEvent args)
    {
        var ident = Spawn(null, Transform(uid).Coordinates);

        _metaData.SetEntityName(ident, "identity");
        QueueIdentityUpdate(uid);
        _container.Insert(ident, component.IdentityEntitySlot);
    }

    /// <summary>
    ///     Queues an identity update to the start of the next tick.
    /// </summary>
    public void QueueIdentityUpdate(EntityUid uid)
    {
        _queuedIdentityUpdates.Add(uid);
    }

    // WWDP simple public API
    public string GetEntityIdentity(EntityUid uid)
    {
        var representation = GetIdentityRepresentation(uid);
        var name = GetIdentityName(uid, representation);

        return name;
    }
    // WWDP edit end

    #region Private API

    /// <summary>
    ///     Updates the metadata name for the id(entity) from the current state of the character.
    /// </summary>
    private void UpdateIdentityInfo(EntityUid uid, IdentityComponent identity)
    {
        if (identity.IdentityEntitySlot.ContainedEntity is not { } ident)
            return;

        var representation = GetIdentityRepresentation(uid);
        var name = GetIdentityName(uid, representation);

        // Clone the old entity's grammar to the identity entity, for loc purposes.
        if (TryComp<GrammarComponent>(uid, out var grammar))
        {
            var identityGrammar = EnsureComp<GrammarComponent>(ident);
            identityGrammar.Attributes.Clear();

            foreach (var (k, v) in grammar.Attributes)
            {
                identityGrammar.Attributes.Add(k, v);
            }

            // If presumed name is null and we're using that, we set proper noun to be false ("the old woman")
            if (name != representation.TrueName && representation.PresumedName == null)
                identityGrammar.ProperNoun = false;

            Dirty(ident, identityGrammar);
        }

        if (name == Name(ident))
            return;

        _metaData.SetEntityName(ident, name);

        _adminLog.Add(LogType.Identity, LogImpact.Medium, $"{ToPrettyString(uid)} changed identity to {name}");
        var identityChangedEvent = new IdentityChangedEvent(uid, ident);
        RaiseLocalEvent(uid, ref identityChangedEvent);
        SetIdentityCriminalIcon(uid);
    }

    private string GetIdentityName(EntityUid target, IdentityRepresentation representation)
    {
        var ev = new SeeIdentityAttemptEvent();

        RaiseLocalEvent(target, ev);
        return representation.ToStringKnown(!ev.Cancelled);
    }

    /// <summary>
    ///     When the identity of a person is changed, searches the criminal records to see if the name of the new identity
    ///     has a record. If the new name has a criminal status attached to it, the person will get the criminal status
    ///     until they change identity again.
    /// </summary>
    private void SetIdentityCriminalIcon(EntityUid uid)
    {
        _criminalRecordsConsole.CheckNewIdentity(uid);
    }

    /// <summary>
    ///     Gets an 'identity representation' of an entity, with their true name being the entity name
    ///     and their 'presumed name' and 'presumed job' being the name/job on their ID card, if they have one.
    /// </summary>
    private IdentityRepresentation GetIdentityRepresentation(EntityUid target,
        InventoryComponent? inventory=null,
        HumanoidAppearanceComponent? appearance=null)
    {
        int age = 18;
        Gender gender = Gender.Epicene;
        string species = SharedHumanoidAppearanceSystem.DefaultSpecies;

        // Always use their actual age and gender, since that can't really be changed by an ID.
        if (Resolve(target, ref appearance, false))
        {
            gender = appearance.Gender;
            age = appearance.Age;
            species = appearance.Species;
        }

        var ageString = _humanoid.GetAgeRepresentation(species, age);
        var trueName = Name(target);
        if (!Resolve(target, ref inventory, false))
            return new(trueName, gender, ageString, string.Empty);

        string? presumedJob = null;
        string? presumedName = null;

        // Get their name and job from their ID for their presumed name.
        if (_idCard.TryFindIdCard(target, out var id))
        {
            presumedName = string.IsNullOrWhiteSpace(id.Comp.FullName) ? null : id.Comp.FullName;
            presumedJob = id.Comp.LocalizedJobTitle?.ToLowerInvariant();
        }

        // If it didn't find a job, that's fine.
        return new(trueName, gender, ageString, presumedName, presumedJob);
    }

    // Goobstation - Update component state on component toggle
    private void BlockerUpdateIdentity(EntityUid uid, IdentityBlockerComponent component, EntityEventArgs args)
    {
        var target = uid;

        if (_inventorySystem.TryGetContainingEntity(uid, out var containing))
            target = containing.Value;

        QueueIdentityUpdate(target);
    }

    #endregion
}