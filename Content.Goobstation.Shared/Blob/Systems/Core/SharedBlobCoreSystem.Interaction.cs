// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Common.CCVar;
using Content.Goobstation.Shared.Blob.Components;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared.Interaction;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.SubFloor;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Threading;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Blob.Systems.Core;

public abstract partial class SharedBlobCoreSystem
{
    private bool _canGrowInSpace = true;

    private BlobInteractJob _job; // j*b

    private void InitializeInteraction()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobObserverControllerComponent, AfterInteractEvent>(OnInteractController);
        SubscribeLocalEvent<BlobCoreComponent, UserActivateInWorldEvent>(OnInteractTarget);
        SubscribeLocalEvent<BlobCoreComponent, GetUsedEntityEvent>(OnGetUsedEntityEvent);

        Subs.CVar(_cfg, GoobCVars.BlobCanGrowInSpace, value => _canGrowInSpace = value, true);

        _job = new() { System = this };
    }

    private void BlobInteract(Entity<BlobCoreComponent> core, InteractEvent args)
    {
        if (TerminatingOrDeleted(core)
            || !args.ClickLocation.IsValid(EntityManager))
            return;

        var location = args.ClickLocation.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapMan);
        var gridUid = _transform.GetGrid(location);

        if (!TryComp<MapGridComponent>(gridUid, out var grid)
            || !TryFindNearBlobTile(location, (gridUid.Value, grid), out var fromTile))
            return;

        var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);

        Entity<BlobNodeComponent>? node = null;
        if (core.Comp.TilesRadiusLimit != null)
        {
            TryGetNearNode(location, out node, core.Comp.TilesRadiusLimit.Value);

            if (node == null)
            {
                _popup.PopupCoordinates(Loc.GetString("blob-target-nearby-not-node"),
                    location,
                    args.User,
                    PopupType.Large);
                return;
            }
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
                && !HasComp<SubFloorHideComponent>(args.Target))
                anchoredTarget = targetEntity;

            // If there's a blob tile here, we can't grow new tiles on top
            if (_tile.HasComp(targetEntity))
                growTile = false;
        }

        if (anchoredTarget != null)
        {
            BlobTargetAttack(core, fromTile.Value, anchoredTarget.Value);
            return;
        }

        // Handle target attack on an entity.
        // Only hard objects should be attacked.
        if (args.Target != null)
        {
            // Things that we can't attack, including our own tiles.
            if (!HasComp<DamageableComponent>(args.Target)
                || HasComp<ItemComponent>(args.Target)
                || HasComp<BlobMobComponent>(args.Target)
                || _tile.TryComp(args.Target, out var targetComp)
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

        var tileProto = _protoMan.Index(core.Comp.GrowthTile);
        var cost = tileProto.Cost;

        if (targetTileEmpty)
        {
            cost *= 2.5f;

            var plating = _tileDefinitionManager["Plating"];
            var platingTile = new Tile(plating.TileId);
            _mapSystem.SetTile(gridUid.Value, grid, location, platingTile);
        }

        if (!TryUseAbility(core, cost, location))
            return;

        _blobTile.TransformBlobTile(null,
            core,
            node,
            core.Comp.GrowthTile,
            location);

        core.Comp.NextAction = _gameTiming.CurTime + TimeSpan.FromSeconds(Math.Abs(core.Comp.GrowRate));
    }

    private readonly List<Vector2i> _directions = new()
    {
        Vector2i.Up,
        Vector2i.Down,
        Vector2i.Left,
        Vector2i.Right,
    };

    public bool TryFindNearBlobTile(
        EntityCoordinates coords,
        Entity<MapGridComponent> grid,
        [NotNullWhen(true)] out EntityUid? tile)
    {
        tile = null;

        foreach (var indices in _directions)
        {
            var uid = _mapSystem.GetAnchoredEntities(grid, grid, indices)
                .Where(_tile.HasComponent)
                .FirstOrNull();

            // Don't count dead tiles
            if (uid == null
                || _tile.Comp(uid.Value).Core == null)
                continue;

            tile = uid;
            return true;
        }

        return false;
    }

    private void BlobTargetAttack(Entity<BlobCoreComponent> ent, Entity<BlobTileComponent?> from, EntityUid target)
    {
        if (!TryUseAbility(ent, ent.Comp.AttackCost, Transform(target).Coordinates))
            return;

        _blobTile.DoLunge(from, target);

        ent.Comp.NextAction = _gameTiming.CurTime + TimeSpan.FromSeconds(Math.Abs(ent.Comp.AttackRate));
        _audio.PlayPvs(ent.Comp.AttackSound, from, AudioParams.Default);

        // Apply damage and EntityEffects
        var currentChem = _protoMan.Index(ent.Comp.CurrentChemical);
        _damageable.TryChangeDamage(target, ent.Comp.AttackDamage + currentChem.Damage);

        var effectArgs = new EntityEffectBaseArgs(target, EntityManager);
        foreach (var effect in currentChem.Effects)
        {
            effect.Effect(effectArgs);
        }
    }

    public void Interact(Entity<BlobCoreComponent> ent, AfterInteractEvent args, bool withdrawPoints = true)
    {
        if (args.Handled
            || _gameTiming.CurTime < ent.Comp.NextAction
            || !args.ClickLocation.IsValid(EntityManager))
            return;

        args.Handled = true;

        _job.Core = (ent.Owner, ent.Comp);
        _job.Args = args;
        _parallel.ProcessNow(_job);
    }

    private void OnInteractTarget(Entity<BlobCoreComponent> ent, ref UserActivateInWorldEvent args)
    {
        var ev = new AfterInteractEvent(args.User, EntityUid.Invalid, args.Target, Transform(args.Target).Coordinates, true);
        Interact((ent.Owner, ent.Comp), ev);
        args.Handled = ev.Handled;
    }

    private void OnInteractController(Entity<BlobObserverControllerComponent> ent, ref AfterInteractEvent args)
    {
        if (!_core.TryComp(ent.Comp.BlobCore, out var coreComp))
            return;

        var ev = new AfterInteractEvent(args.User, EntityUid.Invalid, args.Target, args.ClickLocation, true);
        Interact((ent.Comp.BlobCore, coreComp), ev);
        args.Handled = ev.Handled;
    }

    private void OnGetUsedEntityEvent(Entity<BlobCoreComponent> ent, ref GetUsedEntityEvent args)
    {
        if (ent.Comp.Controller != null)
            args.Used = ent.Comp.Controller;
    }

    private record struct BlobInteractJob : IRobustJob
    {
        public required SharedBlobCoreSystem System;

        public Entity<BlobCoreComponent> Core;
        public InteractEvent Args;

        public void Execute()
        {
            System.BlobInteract(Core, Args);
        }
    }
}
