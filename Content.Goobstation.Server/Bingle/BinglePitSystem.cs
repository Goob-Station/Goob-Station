using System;
using System.Numerics;
using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Content.Server.Stunnable;
using Content.Shared.Destructible;
using Content.Shared.Destructible;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.Humanoid;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Destructible;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Humanoid;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.Bingle;

public sealed class BinglePitSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly BingleSystem _bingleSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StepTriggerSystem _step = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ITileDefinitionManager _tiledef = default!;
    [Dependency] private readonly TileSystem _tile = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BinglePitComponent, StepTriggeredOffEvent>(OnStepTriggered);
        SubscribeLocalEvent<BinglePitComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<BinglePitComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<BinglePitComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BinglePitComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<BinglePitComponent, EntRemovedFromContainerMessage>(OnRemovedFromContainer);
        SubscribeLocalEvent<BinglePitFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
    }

    private void OnRemovedFromContainer(Entity<BinglePitComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        RemCompDeferred<StunnedComponent>(args.Entity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var uid, out var falling))
        {
            if (_timing.CurTime < falling.NextDeletionTime)
                continue;

            _containerSystem.Insert(uid, falling.Pit.Pit);
            EnsureComp<StunnedComponent>(uid); // used stunned to prevent any funny being done inside the pit
            RemCompDeferred(uid, falling);
        }
    }

    private void OnInit(EntityUid uid, BinglePitComponent component, MapInitEvent args)
    {
        if (!Transform(uid).Coordinates.IsValid(EntityManager))
            QueueDel(uid);
        component.Pit = _containerSystem.EnsureContainer<Container>(uid, "pit");
    }

    private void OnStepTriggered(EntityUid uid, BinglePitComponent component, ref StepTriggeredOffEvent args)
    {
        // dont accept if they are already falling
        if (HasComp<BinglePitFallingComponent>(args.Tripper))
            return;

        // Needs to be at level two or above to allow anything alive.
        if (HasComp<MobStateComponent>(args.Tripper) && component.Level < 2)
            return;

        // allow dead bingles
        if (HasComp<BingleComponent>(args.Tripper))
            if(TryComp<MobStateComponent>(args.Tripper, out var mobState) && mobState.CurrentState == MobState.Alive)
                return;

        StartFalling(uid, component, args.Tripper);

        if (component.BinglePoints >=( component.SpawnNewAt * component.Level))
        {
            SpawnBingle(uid, component);
            component.BinglePoints -= ( component.SpawnNewAt * component.Level);
        }
    }

    private void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper, bool playSound = true)
    {
        if (TryComp<MobStateComponent>(tripper, out var mobState) && mobState.CurrentState is MobState.Alive or MobState.Critical)
            component.BinglePoints += component.PointsForAlive;
        else
            component.BinglePoints++;
        if (HasComp<HumanoidAppearanceComponent>(tripper))
            component.BinglePoints += component.SpawnNewAt * component.Level; // trowing a humanoid in the pit  will spawn a new bingle

        if (HasComp<BingleComponent>(tripper))
            component.BinglePoints += (component.SpawnNewAt * component.Level) / 4; //recycling a bingle returns a quarter bingle.

        if (TryComp<PullableComponent>(tripper, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(tripper, pullable, ignoreGrab: true);

        var fall = EnsureComp<BinglePitFallingComponent>(tripper);
        fall.Pit = component;
        fall.NextDeletionTime = _timing.CurTime + fall.DeletionTime;
        _stun.TryKnockdown(tripper, fall.DeletionTime, false);

        if (playSound)
            _audio.PlayPvs(component.FallingSound, uid);

    }

    private void OnStepTriggerAttempt(EntityUid uid, BinglePitComponent component, ref StepTriggerAttemptEvent args)
        => args.Continue = true;

    public void SpawnBingle(EntityUid uid, BinglePitComponent component)
    {
        Spawn(component.GhostRoleToSpawn, Transform(uid).Coordinates);
        OnSpawnTile(uid,component.Level*2, "FloorBingle");

        component.MinionsMade++;
        if (component.MinionsMade >= component.UpgradeMinionsAfter)
        {
            component.MinionsMade = 0;
            component.Level++;
            UpgradeBingles(uid, component);
        }
    }

    public void UpgradeBingles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<BingleComponent>();
        while (query.MoveNext(out var queryUid, out var queryBingleComp))
            if (queryBingleComp.MyPit != null && queryBingleComp.MyPit.Value == uid)
                _bingleSystem.UpgradeBingle(queryUid, queryBingleComp);

        if (component.Level <= component.MaxSize)
            ScaleUpPit(uid, component);

        // make max-size bingle pit ignore gravity
        if (component.Level == component.MaxSize)
            _step.SetIgnoreWeightless(uid, true);

        _popup.PopupEntity(Loc.GetString("bingle-pit-grow"), uid);
    }

    private void OnDestruction(EntityUid uid, BinglePitComponent component, DestructionEventArgs args)
    {
        if (component.Pit != null)
            foreach (var pitUid in _containerSystem.EmptyContainer(component.Pit))
            {
                RemComp<StunnedComponent>(pitUid);
                _stun.TryKnockdown(pitUid, TimeSpan.FromSeconds(2), false);
            }

        RemoveAllBingleGhostRoles(uid, component);//remove all unclaimed ghost roles when pit is destroyed

        //Remove all falling when pit is destroyed, in the small chance someone is in between start and insert
        var query = EntityQueryEnumerator<BinglePitFallingComponent>();
        while (query.MoveNext(out var fallingUid, out var fallingComp))
            RemCompDeferred(fallingUid, fallingComp);
    }

    public void RemoveAllBingleGhostRoles(EntityUid uid, BinglePitComponent component)
    {
        var query = EntityQueryEnumerator<GhostRoleMobSpawnerComponent>();
        while (query.MoveNext(out var queryGRMSUid, out var queryGRMScomp))
            if (queryGRMScomp.Prototype == "MobBingle")
                if (Transform(uid).Coordinates == Transform(queryGRMSUid).Coordinates)
                    QueueDel(queryGRMSUid); // remove any unspanned bingle when pit is destroyed
    }
    private void OnAttacked(EntityUid uid, BinglePitComponent component, AttackedEvent args)
    {
        if (_containerSystem.ContainsEntity(uid, args.User))
            EnsureComp<StunnedComponent>(args.User);
    }

    private void OnUpdateCanMove(EntityUid uid, BinglePitFallingComponent component, UpdateCanMoveEvent args)
        => args.Cancel();

    private void ScaleUpPit(EntityUid uid, BinglePitComponent component)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearanceComponent))
            appearanceComponent = _entityManager.EnsureComponent<AppearanceComponent>(uid);
        var appearance = _entityManager.System<AppearanceSystem>();
        _entityManager.EnsureComponent<ScaleVisualsComponent>(uid);

        appearance.SetData(uid, ScaleVisuals.Scale, Vector2.One * component.Level, appearanceComponent);
    }

    private void OnRoundEndTextAppend(RoundEndTextAppendEvent ev)
    {
        var pits = new List<Entity<BinglePitComponent>>();
        var query = AllEntityQuery<BinglePitComponent>();

        while (query.MoveNext(out var uid, out var comp))
            pits.Add((uid, comp));

        if (pits.Count == 0)
            return;

        ev.AddLine("");

        foreach (var ent in pits)
        {
            var (uid, comp) = ent;

            var location = "Unknown";
            var mapCoords = _transform.ToMapCoordinates(Transform(uid).Coordinates);
            if (_navMap.TryGetNearestBeacon(mapCoords, out var beacon, out _))
                location = beacon?.Comp?.Text!;

            var points = comp.BinglePoints + (comp.MinionsMade * comp.SpawnNewAt) * comp.Level;

            ev.AddLine(Loc.GetString("bingle-pit-end-of-round",
                ("location", location),
                ("level", comp.Level),
                ("points", points)));
        }

        ev.AddLine("");
    }

    private void OnSpawnTile(EntityUid uid,
        float radius,
        ProtoId<ContentTileDefinition> floorTile = new())
    {
        var tgtPos = Transform(uid);
        if (tgtPos.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? mapGrid))
            return;

        var tileEnumerator = _map.GetLocalTilesEnumerator(gridUid, mapGrid, new Box2(tgtPos.Coordinates.Position + new Vector2(-radius, -radius), tgtPos.Coordinates.Position + new Vector2(radius, radius)));
        var convertTile = (ContentTileDefinition)_tiledef[floorTile];

        while (tileEnumerator.MoveNext(out var tile))
        {
            if (tile.Tile.TypeId == convertTile.TileId)
                continue;
            if (tile.GetContentTileDefinition().Name != convertTile.Name &&
                _random.Prob(0.1f)) // 10% probability to transform tile
            {
                _tile.ReplaceTile(tile, convertTile);
                _tile.PickVariant(convertTile);
            }
        }

    }

}

