// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Blob;
using Content.Goobstation.Shared.Blob.Components;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Emp;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.SubFloor;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Configuration;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.Blob;

public sealed class BlobCoreActionSystem : SharedBlobCoreActionSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly BlobTileSystem _blobTileSystem = default!;
    //[Dependency] private readonly GridFixtureSystem _gridFixture = default!;

    private const double ActionJobTime = 0.005;
    private readonly JobQueue _actionJobQueue = new(ActionJobTime);

    private bool _canGrowInSpace = true;
    private EntityQuery<BlobTileComponent> _tileQuery;
    private EntityQuery<BlobCoreComponent> _blobCoreQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobObserverControllerComponent, AfterInteractEvent>(OnInteractController);
        SubscribeLocalEvent<BlobObserverComponent, UserActivateInWorldEvent>(OnInteractTarget);

        Subs.CVar(_cfg, GoobCVars.BlobCanGrowInSpace, value => _canGrowInSpace = value, true);
        _tileQuery = GetEntityQuery<BlobTileComponent>();
        _blobCoreQuery = GetEntityQuery<BlobCoreComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _actionJobQueue.Process();
    }

    public sealed class BlobMouseActionProcess(
        Entity<BlobObserverComponent> ent,
        Entity<BlobCoreComponent> core,
        BlobCoreActionSystem system,
        InteractEvent args,
        bool spendPoints,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
        protected override async Task<object?> Process()
        {
            system.BlobInteract(ent, core, args, spendPoints);
            return null;
        }
    }

    private void BlobInteract(Entity<BlobObserverComponent> observer, Entity<BlobCoreComponent> core, InteractEvent args, bool spendPoints)
    {
        if (TerminatingOrDeleted(observer)
            || TerminatingOrDeleted(core)
            || !args.ClickLocation.IsValid(EntityManager))
            return;

        var location = args.ClickLocation.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapManager);
        var gridUid = _transform.GetGrid(location);

        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var fromTile = FindNearBlobTile(location, (gridUid.Value, grid));
        if (fromTile == null)
            return;

        var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);

        var node = _blobCoreSystem.GetNearNode(location, core.Comp.TilesRadiusLimit);

        if (node == null)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-nearby-not-node"),
                location,
                args.User,
                PopupType.Large);
            return;
        }

        bool growTile = true;

        // First we try to attack some structure on that tile.
        var anchored = _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices);
        EntityUid? anchoredTarget = null;
        foreach (var targetEntity in anchored)
        {
            if (TryComp<PhysicsComponent>(targetEntity, out var physics)
                && physics is { Hard: true, CanCollide: true }
                && HasComp<DamageableComponent>(targetEntity)
                && !HasComp<SubFloorHideComponent>(args.Target)
                && !_tileQuery.HasComp(targetEntity))
                anchoredTarget = targetEntity;

            // If there's a blob tile here, we can't grow new tiles on top
            if (_tileQuery.HasComp(targetEntity))
                growTile = false;
        }

        if (anchoredTarget != null)
        {
            BlobTargetAttack(core, fromTile.Value, anchoredTarget.Value);
            return;
        }

        // Handle target attack on an entity.
        // Only hard objects should be attacked.
        if (args.Target != null
            && TryComp<PhysicsComponent>(args.Target, out var physicsTarget)
            && physicsTarget is { Hard: true, CanCollide: true })
        {
            // Things that we can't attack, including our own tiles.
            if (!HasComp<DamageableComponent>(args.Target)
                || HasComp<ItemComponent>(args.Target)
                || HasComp<BlobMobComponent>(args.Target)
                || _tileQuery.TryComp(args.Target, out var targetComp)
                && targetComp.Core != null)
                return;

            BlobTargetAttack(core, fromTile.Value, args.Target.Value);
            return;
        }

        if (!growTile)
            return;

        var targetTileEmpty = false;
        if (targetTile.Tile.IsEmpty)
        {
            if (!_canGrowInSpace)
                return;

            targetTileEmpty = true;
        }

        var cost = core.Comp.BlobTileCosts[BlobTileType.Normal];
        if (targetTileEmpty)
        {
            cost *= 2.5f;

            var plating = _tileDefinitionManager["Plating"];
            var platingTile = new Tile(plating.TileId);
            _mapSystem.SetTile(gridUid.Value, grid, location, platingTile);
        }

        if (spendPoints)
        {
            if (!_blobCoreSystem.TryUseAbility(core, cost, location))
                return;
        }

        _blobCoreSystem.TransformBlobTile(null,
            core,
            node,
            BlobTileType.Normal,
            location);

        core.Comp.NextAction = _gameTiming.CurTime + GCd + TimeSpan.FromSeconds(Math.Abs(core.Comp.GrowRate));
    }

    private EntityUid? FindNearBlobTile(EntityCoordinates coords, Entity<MapGridComponent> grid)
    {
        var mobTile = _mapSystem.GetTileRef(grid, grid, coords);

        var adjacentTiles = new[]
        {
            mobTile.GridIndices.Offset(Direction.East),
            mobTile.GridIndices.Offset(Direction.West),
            mobTile.GridIndices.Offset(Direction.North),
            mobTile.GridIndices.Offset(Direction.South),
        };

        foreach (var indices in adjacentTiles)
        {
            var uid = _mapSystem.GetAnchoredEntities(grid, grid, indices)
                .Where(_tileQuery.HasComponent)
                .FirstOrNull();

            // Don't count dead tiles
            if (uid == null || _tileQuery.Comp(uid.Value).Core == null)
                continue;

            return uid;
        }

        return null;
    }

    private void BlobTargetAttack(Entity<BlobCoreComponent> ent, Entity<BlobTileComponent?> from, EntityUid target)
    {
        if (ent.Comp.Observer == null)
            return;

        if (!_blobCoreSystem.TryUseAbility(ent, ent.Comp.AttackCost, Transform(target).Coordinates))
            return;

        _blobTileSystem.DoLunge(from, target);
        _damageableSystem.TryChangeDamage(target, ent.Comp.ChemDamageDict[ent.Comp.CurrentChem]);

        switch (ent.Comp.CurrentChem)
        {
            case BlobChemType.ExplosiveLattice:
                _explosionSystem.QueueExplosion(target, ent.Comp.BlobExplosive, 4, 1, 6, maxTileBreak: 0);
                break;
            case BlobChemType.ElectromagneticWeb:
            {
                if (_random.Prob(0.2f))
                    _empSystem.EmpPulse(_transform.GetMapCoordinates(target), 3f, 50f, 3f);
                break;
            }
            case BlobChemType.BlazingOil:
            {
                if (TryComp<FlammableComponent>(target, out var flammable))
                {
                    flammable.FireStacks += 2;
                    _flammable.Ignite(target, from, flammable);
                }

                break;
            }
        }

        ent.Comp.NextAction = _gameTiming.CurTime + GCd + TimeSpan.FromSeconds(Math.Abs(ent.Comp.AttackRate));
        _audioSystem.PlayPvs(ent.Comp.AttackSound, from, AudioParams.Default);
    }

    private static readonly TimeSpan GCd = TimeSpan.FromMilliseconds(333); // GCD?

    public void OnInteract(EntityUid uid, BlobObserverComponent observerComponent, AfterInteractEvent args, bool spendPoints = true)
    {
        if (args.Target == args.User)
            return;

        if (observerComponent.Core == null ||
            !_blobCoreQuery.TryComp(observerComponent.Core.Value, out var blobCoreComponent))
            return;

        if (_gameTiming.CurTime < blobCoreComponent.NextAction)
            return;

        var location = args.ClickLocation;
        if (!location.IsValid(EntityManager))
            return;

        args.Handled = true;
        blobCoreComponent.NextAction = _gameTiming.CurTime + GCd;

        _actionJobQueue.EnqueueJob(new BlobMouseActionProcess(
            (uid,observerComponent),
            (observerComponent.Core.Value, blobCoreComponent),
            this,
            args,
            spendPoints,
            ActionJobTime
        ));
    }
    private void OnInteractTarget(Entity<BlobObserverComponent> ent, ref UserActivateInWorldEvent args)
    {
        var ev = new AfterInteractEvent(args.User, EntityUid.Invalid, args.Target, Transform(args.Target).Coordinates, true);
        OnInteract(ent, ent, ev); // proxy?
        args.Handled = ev.Handled;
    }
    private void OnInteractController(Entity<BlobObserverControllerComponent> ent, ref AfterInteractEvent args)
    {
        var ev = new AfterInteractEvent(args.User, EntityUid.Invalid, args.Target, args.ClickLocation, true);
        OnInteract(ent.Comp.Blob, ent.Comp.Blob, ev); // proxy?
        args.Handled = ev.Handled;
    }
}
