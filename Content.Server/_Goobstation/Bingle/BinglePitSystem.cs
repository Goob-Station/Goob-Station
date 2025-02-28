using System.Linq;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Content.Server.Stunnable;
using Content.Shared.Ghost.Roles.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Destructible;
using Content.Shared.Stunnable;
using Content.Shared.Humanoid;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Movement.Events;
using Content.Shared._Goobstation.Bingle;
using Content.Shared.Popups;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Server.GameTicking;
using Content.Server.Pinpointer;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Bingle;

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
    [Dependency] protected readonly IRobustRandom Random = default!;
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
        SubscribeLocalEvent<BinglePitFallingComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEndTextAppend);
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
            EnsureComp<StunnedComponent>(uid); // used stuned to prevent any funny being done inside the pit
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
        // dont swallow bingles
        if (HasComp<BingleComponent>(args.Tripper))
            if(TryComp<MobStateComponent>(args.Tripper, out var mobState) && mobState.CurrentState == MobState.Alive)
                return;
        // need to be at levl 2 or above to swallow anything alive
        if (HasComp<MobStateComponent>(args.Tripper) && component.Level < 2)
            return;
        if (HasComp<BinglePitFallingComponent>(args.Tripper))
            return;

        StartFalling(uid, component, args.Tripper);

        if (component.BinglePoints >=( component.SpawnNewAt * component.Level))
        {
            SpawnBingle(uid, component);
            component.BinglePoints -= ( component.SpawnNewAt * component.Level);
        }
    }

    public void StartFalling(EntityUid uid, BinglePitComponent component, EntityUid tripper, bool playSound = true)
    {
        if (TryComp<MobStateComponent>(tripper, out var mobState)&& mobState.CurrentState is not MobState.Dead)
        {
            component.BinglePoints += component.PointsForAlive;
            if (HasComp<HumanoidAppearanceComponent>(tripper))
                component.BinglePoints += component.AdditionalPointsForHuman;
        }

        if (TryComp<PullableComponent>(tripper, out var pullable) && pullable.BeingPulled)
            _pulling.TryStopPull(tripper, pullable);

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
        OnSpawnTile(uid,(int)component.Level*3, (int)component.Level, (int)component.Level*2);

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

        RemoveAllBingleGhostRoles(uid, component);//remove all unclaimed ghostroles when pit is destroyed

        //Remove all falling when pit is destroyed, in the small chance somone is inbetween start and insert
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
                    QueueDel(queryGRMSUid); // remove any unspawned bngle when pit is destroyed
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

        var query = AllEntityQuery<BinglePitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // neares beacon
            var location = "Unknown";
            var mapCoords = _transform.ToMapCoordinates(Transform(uid).Coordinates);
            if (_navMap.TryGetNearestBeacon(mapCoords, out var beacon, out _))
                location = beacon?.Comp?.Text!;

            var points = comp.BinglePoints + (comp.MinionsMade * comp.SpawnNewAt) * comp.Level;

            ev.AddLine(Loc.GetString("binge-pit-end-of-round",
                ("location", location),
                ("level", comp.Level),
                ("points", points)));

        }

    }

    private void OnSpawnTile(EntityUid uid,int maxRange = 1, int minRange = 1, int amount = 1 , string floorTile = "FloorBingle")
    {
        if (!TryComp<MapGridComponent>(Transform(uid).GridUid, out var grid))
            return;

        var localpos = Transform(uid).Coordinates;
        var tilerefs = _map.GetLocalTilesIntersecting(
                uid,
                grid,
                new Box2(localpos.Position + new Vector2(-maxRange, -maxRange), localpos.Position + new Vector2(maxRange, maxRange)))
            .ToList();

        if (tilerefs.Count == 0)
            return;

        var physQuery = GetEntityQuery<PhysicsComponent>();
        var resultList = new List<TileRef>();

        while (resultList.Count < amount)
        {
            if (tilerefs.Count == 0)
                break;

            var tileref = Random.Pick(tilerefs);
            var distance = MathF.Sqrt(MathF.Pow(tileref.X - localpos.X, 2) + MathF.Pow(tileref.Y - localpos.Y, 2));

            //cut outer & inner circle
            if (distance > maxRange || distance < minRange)
            {
                tilerefs.Remove(tileref);
                continue;
            }

            resultList.Add(tileref);
        }

        if (!_tiledef.TryGetDefinition(floorTile,  out  var tile))
            return;

        foreach (var tileref in resultList)
        {
            _tile.ReplaceTile(tileref, (ContentTileDefinition) tile);
        }

    }

}
