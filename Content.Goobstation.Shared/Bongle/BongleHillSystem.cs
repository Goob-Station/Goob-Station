using Content.Shared.Chat;
using Content.Shared.Destructible;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Bongle;

public sealed partial class BongleHillSystem : EntitySystem
{
    [Dependency] private BongleSystem _bongle = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private MobStateSystem _mob = default!;
    [Dependency] private EntityWhitelistSystem _whitelist = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;

    [SubscribeLocalEvent]
    private void OnStartup(Entity<BongleHillComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.Container = _container.EnsureContainer<Container>(ent.Owner, ent.Comp.ContainerId);
    }

    [SubscribeLocalEvent]
    private void OnStepTriggerAttempt(Entity<BongleHillComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    [SubscribeLocalEvent]
    private void OnStepTriggered(Entity<BongleHillComponent> ent, ref StepTriggeredOffEvent args)
    {
        if (!_net.IsServer)
            return;

        var tripper = args.Tripper;

        if (ent.Comp.IncapacitatedOnly && HasComp<MobStateComponent>(tripper) && !_mob.IsIncapacitated(tripper))
            return;

        if (!_container.Insert(tripper, ent.Comp.Container))
            return;

        ent.Comp.Points += GetPoints(ent.Comp, tripper);
        _audio.PlayPvs(ent.Comp.SuckedInSound, ent);

        if (_mob.IsDead(tripper) && _whitelist.IsWhitelistPass(ent.Comp.TriumphWhitelist, tripper) && !_whitelist.IsWhitelistPass(ent.Comp.TriumphBlacklist, tripper))
        {
            ent.Comp.BingleCorpses++;

            if (ent.Comp.BingleCorpses >= ent.Comp.CorpsesForTriumph)
            {
                ent.Comp.BingleCorpses -= ent.Comp.CorpsesForTriumph;
                _bongle.BoostBongles(ent.Comp.CapacityIncrease, ent.Comp.SpitIncrease);
            }
        }

        if (ent.Comp.Points < ent.Comp.PointsToSpread)
            return;

        ent.Comp.Points -= ent.Comp.PointsToSpread;
        Spread(ent);
    }

    [SubscribeLocalEvent]
    private void OnDestruction(Entity<BongleHillComponent> ent, ref DestructionEventArgs args)
    {
        _container.EmptyContainer(ent.Comp.Container);
    }

    private void Spread(Entity<BongleHillComponent> ent)
    {
        var center = Transform(ent.Owner).Coordinates;

        // Just copied from wraith void portal, checks for unobstructed tile to spawn in.
        EntityCoordinates coords = default;
        var attempts = 0;
        const int maxAttempts = 500; // just like void portal, no reason for this to not be hardcoded.

        do
        {
            attempts++;

            coords = center.Offset(_random.NextVector2(ent.Comp.SpreadMinRange, ent.Comp.SpreadMaxRange));

            // Temporary spawn to check if its obstructed
            var tempEnt = SpawnAtPosition(ent.Comp.GhostHillProto, coords);
            bool obstructed = _physics.GetEntitiesIntersectingBody(tempEnt, (int) CollisionGroup.Impassable).Count > 0;
            QueueDel(tempEnt);

            if (!obstructed)
                break;

        } while (attempts < maxAttempts);

        var hillCount = 0;
        var ascended = false;
        var query = EntityQueryEnumerator<BongleHillComponent>();
        while (query.MoveNext(out _, out var hill))
        {
            hillCount++;
            ascended |= hill.Ascended;
        }

        _bongle.UpgradeBongles();

        if (!ascended && hillCount >= ent.Comp.AscensionHillCount)
        {
            _bongle.AscendBongles();
            _chat.DispatchGlobalAnnouncement(Loc.GetString("heretic-ascension-bongle"), Name(ent.Owner), true, ent.Comp.AscensionSound, ent.Comp.AnnouncementColor);

            var hills = EntityQueryEnumerator<BongleHillComponent>();
            while (hills.MoveNext(out _, out var ascendedHill))
            {
                ascendedHill.Ascended = true;
            }
        }

        SpawnAtPosition(ent.Comp.HillProto, coords);
        SpawnAtPosition(ent.Comp.BongleProto, coords);
    }

    private int GetPoints(BongleHillComponent comp, EntityUid target)
    {
        foreach (var value in comp.PointValues)
        {
            if (_whitelist.IsWhitelistPass(value.Whitelist, target))
                return value.Points;
        }

        return comp.DefaultPoints;
    }
}
