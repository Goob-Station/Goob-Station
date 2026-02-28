// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Blob;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Server.Construction.Components;
using Content.Server.Destructible;
using Content.Server.Emp;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Blob;

public sealed class BlobTileSystem : SharedBlobTileSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly BlobCoreActionSystem _blobCoreActionSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly EmpSystem _empSystem = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly NpcFactionSystem _npcFactionSystem = default!;

    private EntityQuery<BlobCoreComponent> _blobCoreQuery;
    private EntityQuery<BlobTileComponent> _tileQuery;
    private EntityQuery<BlobObserverComponent> _observerQuery;

    [ValidatePrototypeId<NpcFactionPrototype>]
    private const string BlobFaction = "Blob";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobTileComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<BlobTileComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<BlobTileComponent, BlobTileGetPulseEvent>(OnPulsed);
        SubscribeLocalEvent<BlobTileComponent, EntityTerminatingEvent>(OnTerminate);

        _blobCoreQuery = GetEntityQuery<BlobCoreComponent>();
        _tileQuery = GetEntityQuery<BlobTileComponent>();
        _observerQuery = GetEntityQuery<BlobObserverComponent>();
    }

    private void OnMapInit(Entity<BlobTileComponent> ent, ref MapInitEvent args)
    {
        var faction = EnsureComp<NpcFactionMemberComponent>(ent);
        Entity<NpcFactionMemberComponent?> factionEnt = (ent, faction);

        _npcFactionSystem.ClearFactions(factionEnt, false);
        _npcFactionSystem.AddFaction(factionEnt, BlobFaction, true);

        // make alive - true for npc combat
        EnsureComp<MobStateComponent>(ent);
    }

    private void OnTerminate(EntityUid uid, BlobTileComponent component, EntityTerminatingEvent args)
    {
        if (TerminatingOrDeleted(component.Core))
            return;

        component.Core!.Value.Comp.BlobTiles.Remove(uid);
    }

    private void OnDestruction(EntityUid uid, BlobTileComponent component, DestructionEventArgs args)
    {
        if (
            TerminatingOrDeleted(component.Core) ||
            !_blobCoreQuery.TryComp(component.Core, out var blobCoreComponent)
            )
            return;

        if (blobCoreComponent.CurrentChem == BlobChemType.ElectromagneticWeb)
        {
            _empSystem.EmpPulse(_transform.GetMapCoordinates(uid), 3f, 50f, 3f);
        }
    }

    private readonly List<Vector2i> _directions = new()
    {
        Vector2i.Up,
        Vector2i.Down,
        Vector2i.Left,
        Vector2i.Right,
    };

    private void OnPulsed(EntityUid uid, BlobTileComponent component, BlobTileGetPulseEvent args)
    {
        if (args.Handled
            || !CoreQuery.TryComp(component.Core, out var coreComp))
            return;

        HealTile((uid, component));

        // Automatically grow/attack somewhere by simulating an interaction.
        var xform = Transform(uid);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var curTile = _mapSystem.CoordinatesToTile(xform.GridUid.Value, grid, xform.Coordinates);
        EntityCoordinates? clickLocation = null;

        _random.Shuffle(_directions);
        foreach (var dir in _directions)
        {
            if (CheckTile((xform.GridUid.Value, grid), curTile + dir, out clickLocation))
                break;
        }

        if (clickLocation == null
            || !_observerQuery.TryComp(coreComp.Observer, out var observerComp))
            return;

        var ev = new AfterInteractEvent(coreComp.Observer.Value, EntityUid.Invalid, null, clickLocation.Value, true);
        _blobCoreActionSystem.OnInteract(coreComp.Observer.Value, observerComp, ev, false);
        args.Handled = true;
    }

    private void HealTile(Entity<BlobTileComponent> ent)
    {
        var healCore = new DamageSpecifier();

        foreach (var keyValuePair in ent.Comp.HealthOfPulse.DamageDict)
        {
            healCore.DamageDict.TryAdd(keyValuePair.Key, keyValuePair.Value * 5);
        }

        _damageableSystem.TryChangeDamage(ent, healCore);
    }

    private bool CheckTile(
        Entity<MapGridComponent> grid,
        Vector2i tile,
        [NotNullWhen(true)] out EntityCoordinates? position)
    {
        position = null;
        var picked = _mapSystem.GridTileToLocal(grid, grid, tile);

        // Check it for other blob tiles
        var anchored = _mapSystem.GetAnchoredEntities(grid, grid, picked);
        foreach (var uid in anchored)
        {
            if (_tileQuery.HasComp(uid))
                return false;
        }

        // Don't automatically grow in space
        var targetTile = _mapSystem.GetTileRef(grid, grid, picked);
        if (targetTile.Tile.IsEmpty)
            return false;

        position = picked;
        return true;
    }

    protected override void TryUpgrade(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core, EntityUid observer)
    {
        var coords = Transform(target).Coordinates;

        if (target.Comp.BlobTileType == BlobTileType.Reflective)
            return;

        var nearNode = _blobCoreSystem.GetNearNode(coords, core.Comp.TilesRadiusLimit);
        if (nearNode == null)
            return;

        var ev = new BlobTransformTileActionEvent(
            performer: observer,
            target: coords,
            transformFrom: target.Comp.BlobTileType,
            tileType: BlobTileType.Invalid,
            requireNode: false);

        ev.TileType = ev.TransformFrom switch
        {
            BlobTileType.Normal => BlobTileType.Strong,
            BlobTileType.Strong => BlobTileType.Reflective,
            _ => BlobTileType.Invalid
        };

        RaiseLocalEvent(core, ev);
    }

    public void SwapSpecials(Entity<BlobNodeComponent> from, Entity<BlobNodeComponent> to)
    {
        (from.Comp.BlobFactory, to.Comp.BlobFactory) = (to.Comp.BlobFactory, from.Comp.BlobFactory);
        (from.Comp.BlobResource, to.Comp.BlobResource) = (to.Comp.BlobResource, from.Comp.BlobResource);
        Dirty(from);
        Dirty(to);
    }

    public bool IsEmptySpecial(Entity<BlobNodeComponent> node, BlobTileType tile)
    {
        return tile switch
        {
            BlobTileType.Factory => node.Comp.BlobFactory == null || TerminatingOrDeleted(node.Comp.BlobFactory),
            BlobTileType.Resource => node.Comp.BlobResource == null || TerminatingOrDeleted(node.Comp.BlobResource),
            _ => false
        };
    }

    public void DoLunge(EntityUid from, EntityUid target)
    {
        if(!TransformQuery.TryComp(from, out var userXform))
            return;

        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(userXform));
        localPos = userXform.LocalRotation.RotateVec(localPos);

        RaiseNetworkEvent(new BlobAttackEvent(GetNetEntity(from), GetNetEntity(target), localPos), Filter.Pvs(from));
    }
}
