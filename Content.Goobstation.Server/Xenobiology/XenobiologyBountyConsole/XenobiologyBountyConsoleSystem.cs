using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;
using Content.Server.NameIdentifier;
using Content.Server.Research.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.NameIdentifier;
using Content.Shared.Stacks;
using Content.Shared.Whitelist;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Xenobiology.XenobiologyBountyConsole;

public sealed class XenobiologyBountyConsoleSytem : EntitySystem
{
    [Dependency] private readonly NameIdentifierSystem _nameIdentifier = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ResearchSystem _research = default!;

    [ValidatePrototypeId<NameIdentifierGroupPrototype>]
    private const string BountyNameIdentifierGroup = "Bounty";

    private EntityQuery<StackComponent> _stackQuery;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BoundUIOpenedEvent>(OnBountyConsoleOpened);
        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BountyFulfillMessage>(OnFulfillMessage);
        SubscribeLocalEvent<XenobiologyBountyConsoleComponent, BountySkipMessage>(OnSkipBountyMessage);
        SubscribeLocalEvent<StationXenobiologyBountyDatabaseComponent, MapInitEvent>(OnMapInit);

        _stackQuery = GetEntityQuery<StackComponent>();
        _sawmill = Logger.GetSawmill("xenobio-console");
    }

    private void OnBountyConsoleOpened(Entity<XenobiologyBountyConsoleComponent> console, ref BoundUIOpenedEvent args)
    {
        if (_station.GetOwningStation(console) is not { } station ||
            !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var bountyDb))
            return;

        var untilNextSkip = bountyDb.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(console.Owner, CargoConsoleUiKey.Bounty, new XenobiologyBountyConsoleState(bountyDb.Bounties, bountyDb.History, untilNextSkip));
    }

    private void OnFulfillMessage(Entity<XenobiologyBountyConsoleComponent> console, ref BountyFulfillMessage args)
    {
        if (_station.GetOwningStation(console) is not { } station
            || !TryGetBountyFromId(station, args.BountyId, out var bounty)
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
            || !_research.TryGetClientServer(console, out var server, out var serverComponent)
            || !TryRemoveBounty(station, bounty, false, args.Actor)
            || !TryAddBounty(station))
            return;

        foreach (var bountyEnt in bountyEntities)
            Del(bountyEnt);

        // This has to be casted and rounded because of how the multiplier works.
        _research.ModifyServerPoints(server.Value, bountyProto.RoundedPointsAwarded, serverComponent);
        _audio.PlayPvs(console.Comp.FulfillSound, console);
        _sawmill.Info($"({bounty.Bounty.Id}) Fulfilled - Points: {bountyProto.BasePointsAwarded} -> {bountyProto.RoundedPointsAwarded}");
        RandomizeBountyPointMultiplier(db); // Randomize prices for all bounties - :trol:

        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(console.Owner, CargoConsoleUiKey.Bounty, new XenobiologyBountyConsoleState(db.Bounties, db.History, untilNextSkip));
    }

    private void OnSkipBountyMessage(Entity<XenobiologyBountyConsoleComponent> console, ref BountySkipMessage args)
    {
        if (_station.GetOwningStation(console) is not { } station
            || !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db)
            || _timing.CurTime < db.NextSkipTime
            || !TryGetBountyFromId(station, args.BountyId, out var bounty)
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

        if (!TryRemoveBounty(station, bounty, true, args.Actor))
            return;

        FillBountyDatabase(station);
        db.NextSkipTime = _timing.CurTime + db.SkipDelay;
        var untilNextSkip = db.NextSkipTime - _timing.CurTime;
        _uiSystem.SetUiState(console.Owner, CargoConsoleUiKey.Bounty, new XenobiologyBountyConsoleState(db.Bounties, db.History, untilNextSkip));
    }

    private void OnMapInit(Entity<StationXenobiologyBountyDatabaseComponent> database, ref MapInitEvent args) =>
        FillBountyDatabase(database!);

    /// <summary>
    /// Fills up the bounty database with random bounties.
    /// </summary>
    private void FillBountyDatabase(Entity<StationXenobiologyBountyDatabaseComponent?> database)
    {
        if (!Resolve(database, ref database.Comp))
            return;

        while (database.Comp.Bounties.Count < database.Comp.MaxBounties)
        {
            if (!TryAddBounty(database))
                break;
        }

        RandomizeBountyPointMultiplier(database.Comp);
        UpdateBountyConsoles();
    }

    public void RerollBountyDatabase(Entity<StationXenobiologyBountyDatabaseComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Bounties.Clear();
        FillBountyDatabase(entity);
    }
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

    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, StationXenobiologyBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        // todo: consider making the cargo bounties weighted.
        var allBounties = _proto.EnumeratePrototypes<XenobiologyBountyPrototype>()
            .Where(proto => proto.Group == component.Group)
            .ToList();

        var filteredBounties = allBounties
            .Where(proto => component.Bounties.All(bounty => bounty.Bounty != proto.ID))
            .ToList();

        var pool = filteredBounties.Count == 0 ? allBounties : filteredBounties;
        var bounty = _random.Pick(pool);
        return TryAddBounty(uid, bounty, component);
    }

    [PublicAPI]
    public bool TryAddBounty(EntityUid uid, string bountyId, StationXenobiologyBountyDatabaseComponent? component = null)
    {
        return _proto.TryIndex<XenobiologyBountyPrototype>(bountyId, out var bounty) && TryAddBounty(uid, bounty, component);
    }

    private bool TryAddBounty(EntityUid uid, XenobiologyBountyPrototype bounty, StationXenobiologyBountyDatabaseComponent? component = null)
    {
        if (!Resolve(uid, ref component)
            || component.Bounties.Count >= component.MaxBounties)
            return false;

        _nameIdentifier.GenerateUniqueName(uid, BountyNameIdentifierGroup, out var randomVal);
        var newBounty = new XenobiologyBountyData(bounty, randomVal);

        if (component.Bounties.Any(bountyData => bountyData.Id == newBounty.Id))
        {
            Log.Error("Failed to add bounty {ID} because another one with the same ID already existed!", newBounty.Id);
            return false;
        }

        component.Bounties.Add(newBounty);
        component.TotalBounties++;
        return true;
    }

    [PublicAPI]
    public bool TryRemoveBounty(Entity<StationXenobiologyBountyDatabaseComponent?> ent,
        string dataId,
        bool skipped,
        EntityUid? actor = null)
    {
        return TryGetBountyFromId(ent.Owner, dataId, out var data, ent.Comp) && TryRemoveBounty(ent, data, skipped, actor);
    }

    private bool TryRemoveBounty(Entity<StationXenobiologyBountyDatabaseComponent?> ent,
        XenobiologyBountyData data,
        bool skipped,
        EntityUid? actor = null)
    {
        if (!Resolve(ent, ref ent.Comp))
            return false;

        for (var i = 0; i < ent.Comp.Bounties.Count; i++)
        {
            if (ent.Comp.Bounties[i].Id != data.Id)
                continue;

            string? actorName = null;
            if (actor != null)
            {
                var getIdentityEvent = new TryGetIdentityShortInfoEvent(ent.Owner, actor.Value);
                RaiseLocalEvent(getIdentityEvent);
                actorName = getIdentityEvent.Title;
            }

            ent.Comp.History.Add(new XenobiologyBountyHistoryData(data,
                skipped
                    ? CargoBountyHistoryData.BountyResult.Skipped
                    : CargoBountyHistoryData.BountyResult.Completed,
                _timing.CurTime,
                actorName));
            ent.Comp.Bounties.RemoveAt(i);
            return true;
        }

        return false;
    }

    private bool TryGetBountyFromId(
        EntityUid uid,
        string id,
        [NotNullWhen(true)] out XenobiologyBountyData? bounty,
        StationXenobiologyBountyDatabaseComponent? component = null)
    {
        bounty = null;
        if (!Resolve(uid, ref component))
            return false;

        foreach (var bountyData in component.Bounties.Where(bountyData => bountyData.Id == id))
        {
            bounty = bountyData;
            break;
        }

        return bounty != null;
    }

    private void UpdateBountyConsoles()
    {
        var query = EntityQueryEnumerator<XenobiologyBountyConsoleComponent, UserInterfaceComponent>();
        while (query.MoveNext(out var uid, out _, out var ui))
        {
            if (_station.GetOwningStation(uid) is not { } station
                || !TryComp<StationXenobiologyBountyDatabaseComponent>(station, out var db))
                continue;

            var untilNextSkip = db.NextSkipTime - _timing.CurTime;
            _uiSystem.SetUiState((uid, ui), CargoConsoleUiKey.Bounty, new XenobiologyBountyConsoleState(db.Bounties, db.History, untilNextSkip));
        }
    }

    private void RandomizeBountyPointMultiplier(StationXenobiologyBountyDatabaseComponent db)
    {
        foreach (var bounty in db.Bounties)
            RandomizeBountyPointMultiplier(bounty);
    }

    private void RandomizeBountyPointMultiplier(XenobiologyBountyData bounty)
    {
        if (!_proto.TryIndex(bounty.Bounty, out var bountyProto))
            return;

        bounty.CurrentMultiplier = _random.NextFloat(bounty.MinMultiplier, bounty.MaxMultiplier);
        bountyProto.PointsAwarded = bountyProto.BasePointsAwarded * bounty.CurrentMultiplier;
    }
}
