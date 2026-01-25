// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;
using Content.Server.Research.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// This handles the Xenobiology console.
/// </summary>
public sealed class XenobiologyBountyConsoleSystem : EntitySystem
{
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ResearchSystem _research = default!;
    [Dependency] private readonly StationXenobiologyBountyDatabaseSystem _xenoDatabase = default!;

    private EntityQuery<StackComponent> _stackQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BoundUIOpenedEvent>(OnBountyConsoleOpened);
        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BountyFulfillMessage>(OnFulfillMessage);
        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BountySkipMessage>(OnSkipBountyMessage);

        _stackQuery = GetEntityQuery<StackComponent>();
    }

    private void OnBountyConsoleOpened(Entity<XenobiologyBountyConsoleComponent> console, ref BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(console) is not { } station ||
            !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db))
            return;

        UpdateState(console, db);
    }

    private void OnFulfillMessage(Entity<XenobiologyBountyConsoleComponent> console, ref BountyFulfillMessage args)
    {
        if (_station.GetOwningStation(console) is not { } station
            || !_xenoDatabase.TryGetBountyFromId(station, args.BountyId, out var bounty)
            || !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db))
            return;

        if (!IsBountyComplete(args.Actor, bounty, out var bountyEntities))
        {
            if (_timing.CurTime >= console.Comp.NextDenySoundTime)
            {
                console.Comp.NextDenySoundTime = _timing.CurTime + console.Comp.DenySoundDelay;
                _audio.PlayPvs(console.Comp.DenySound, console);
            }

            return;
        }

        if (!_proto.TryIndex(bounty.Bounty, out var bountyProto)
            || bountyProto.PointsAwarded <= 0
            || !_research.TryGetClientServer(console, out var server, out var serverComponent))
            return;

        foreach (var bountyEnt in bountyEntities)
            Del(bountyEnt);

        var pointsToAward = !_proto.TryIndex(bounty.Bounty, out var bp) ? 0 : bp.PointsAwarded;
        _research.ModifyServerPoints(server.Value, (int) pointsToAward, serverComponent);
        _xenoDatabase.TryRemoveBounty(station, bounty, false, args.Actor);

        _audio.PlayPvs(console.Comp.FulfillSound, console);

        UpdateState(console, db);
    }

    private void OnSkipBountyMessage(Entity<XenobiologyBountyConsoleComponent> console, ref BountySkipMessage args)
    {
        if (_station.GetOwningStation(console) is not { } station
            || !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db)
            || _timing.CurTime < db.NextSkipTime
            || !_xenoDatabase.TryGetBountyFromId(station, args.BountyId, out var bounty)
            || args.Actor is not { Valid: true } mob)
            return;

        if (TryComp<AccessReaderComponent>(console, out var accessReaderComponent) &&
            !_access.IsAllowed(mob, console, accessReaderComponent))
        {
            if (_timing.CurTime >= console.Comp.NextDenySoundTime)
            {
                console.Comp.NextDenySoundTime = _timing.CurTime + console.Comp.DenySoundDelay;
                _audio.PlayPvs(console.Comp.DenySound, console);
            }

            return;
        }

        if (!_xenoDatabase.TryRemoveBounty(station, bounty, true, args.Actor))
            return;

        _xenoDatabase.FillBountyDatabase(station);
        db.NextSkipTime = _timing.CurTime + db.SkipDelay;
        UpdateState(console, db);
    }

    #region Bounty Management
    private bool IsBountyComplete(EntityUid entity, XenobiologyBountyData data, out HashSet<EntityUid> bountyEntities)
    {
        if (_proto.TryIndex(data.Bounty, out var proto))
            return IsBountyComplete(entity, proto.Entries, out bountyEntities);

        bountyEntities = [];
        return false;
    }

    public bool IsBountyComplete(EntityUid entity, string id)
    {
        return _proto.TryIndex<XenobiologyBountyPrototype>(id, out var proto) && IsBountyComplete(entity, proto.Entries);
    }

    public bool IsBountyComplete(EntityUid entity, ProtoId<XenobiologyBountyPrototype> prototypeId)
    {
        var prototype = _proto.Index(prototypeId);

        return IsBountyComplete(entity, prototype.Entries);
    }

    private bool IsBountyComplete(EntityUid container, IEnumerable<XenobiologyBountyItemEntry> entries)
    {
        return IsBountyComplete(container, entries, out _);
    }

    private bool IsBountyComplete(EntityUid container, IEnumerable<XenobiologyBountyItemEntry> entries, out HashSet<EntityUid> bountyEntities)
    {
        return IsBountyComplete(GetBountyEntities(container), entries, out bountyEntities);
    }

    /// <summary>
    /// Determines whether the <paramref name="entity"/> meets the criteria for the bounty <paramref name="entry"/>.
    /// </summary>
    /// <returns>true if <paramref name="entity"/> is a valid item for the bounty entry, otherwise false</returns>
    private bool IsValidBountyEntry(EntityUid entity, XenobiologyBountyItemEntry entry)
    {
        if (!_whitelist.IsValid(entry.Whitelist, entity))
            return false;

        return entry.Blacklist == null || !_whitelist.IsValid(entry.Blacklist, entity);
    }

    private bool IsBountyComplete(HashSet<EntityUid> entities, IEnumerable<XenobiologyBountyItemEntry> entries, out HashSet<EntityUid> bountyEntities)
    {
        bountyEntities = [];

        foreach (var entry in entries)
        {
            var count = 0;

            var temp = new HashSet<EntityUid>();
            foreach (var entity in entities.Where(entity => IsValidBountyEntry(entity, entry)))
            {
                count += _stackQuery.CompOrNull(entity)?.Count ?? 1;
                temp.Add(entity);

                if (count >= entry.Amount)
                    break;
            }

            if (count < entry.Amount)
                return false;

            foreach (var ent in temp)
            {
                entities.Remove(ent);
                bountyEntities.Add(ent);
            }
        }

        return true;
    }

    private HashSet<EntityUid> GetBountyEntities(EntityUid uid)
    {
        var entities = new HashSet<EntityUid> { uid };

        if (!TryComp<ContainerManagerComponent>(uid, out var containers))
            return entities;

        foreach (var child in containers.Containers.Values
                     .SelectMany(container => container.ContainedEntities, (_, ent) => GetBountyEntities(ent))
                     .SelectMany(children => children))
            entities.Add(child);

        return entities;
    }

    #endregion

    #region Helpers

    public void UpdateBountyConsoles()
    {
        var query = EntityQueryEnumerator<XenobiologyBountyConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out _, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station
                || !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db))
                continue;

            UpdateState(uid, db);
        }
    }

    private void UpdateState(EntityUid console, StationXenobiologyBountyDatabaseComponent db)
    {
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        var untilNextRefresh = db.NextGlobalMarketRefresh - _timing.CurTime;
        var state = new XenobiologyBountyConsoleState(db.Bounties, db.History, untilNextSkip, untilNextRefresh);

        _xenoDatabase.SortBounties(db);
        _uiSystem.SetUiState(console, CargoConsoleUiKey.Bounty, state);
    }

    #endregion
}
