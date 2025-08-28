// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Events;
using Content.Goobstation.Shared.Blob.Prototypes;
using Content.Goobstation.Shared.Blob.Systems.Core;
using Content.Shared.Mobs.Components;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Blob.Systems;

public abstract class SharedBlobTileSystem : EntitySystem
{
    [Dependency] protected readonly SharedBlobCoreSystem BlobCore = default!;
    [Dependency] protected readonly IPrototypeManager ProtoMan = default!;
    [Dependency] private readonly   SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly   SharedMeleeWeaponSystem _meleeWeaponSystem = default!;
    [Dependency] private readonly   SharedTransformSystem _transform = default!;
    [Dependency] private readonly   SharedPopupSystem _popup = default!;
    [Dependency] private readonly   NpcFactionSystem _npcFactionSystem = default!;

    protected EntityQuery<BlobCoreComponent> CoreQuery;
    protected EntityQuery<BlobTileComponent> TileQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobUpgradeableTileComponent, GetVerbsEvent<AlternativeVerb>>(AddUpgradeVerb);
        SubscribeLocalEvent<BlobTileComponent, EntityTerminatingEvent>(OnTerminate);
        SubscribeLocalEvent<BlobTileComponent, MapInitEvent>(OnMapInit);

        CoreQuery = GetEntityQuery<BlobCoreComponent>();
        TileQuery = GetEntityQuery<BlobTileComponent>();
    }

    /// <summary>
    /// Transforms one blob tile in another type or creates a new one from scratch.
    /// </summary>
    /// <param name="oldTileUid">Uid of the ols tile that's going to get deleted.</param>
    /// <param name="blobCore">Blob core that preformed the transformation. Make sure it isn't came from the BlobTileComponent of the target!</param>
    /// <param name="nearNode">Node will be used in ConnectBlobTile method.</param>
    /// <param name="newBlobTile">Type of a new blob tile.</param>
    /// <param name="coordinates">Coordinates of a new tile.</param>
    /// <seealso cref="ConnectBlobTile"/>
    /// <seealso cref="BlobCoreComponent"/>
    public bool TransformBlobTile(
        Entity<BlobTileComponent>? oldTileUid,
        Entity<BlobCoreComponent> blobCore,
        Entity<BlobNodeComponent>? nearNode,
        ProtoId<BlobTilePrototype> newBlobTile,
        EntityCoordinates coordinates)
    {
        if (oldTileUid != null)
        {
            if (oldTileUid.Value.Comp.Core != blobCore.Owner)
                return false;

            RemoveBlobTile(oldTileUid.Value, blobCore);
        }

        var blobCoreComp = blobCore.Comp;
        var proto = ProtoMan.Index(newBlobTile);
        var blobTileUid = Spawn(proto.SpawnId, coordinates);

        if (!TileQuery.TryGetComponent(blobTileUid, out var blobTileComp))
        {
            Log.Error($"Spawned blob tile {ToPrettyString(blobTileUid)} doesn't have BlobTileComponent!");
            return false;
        }

        ConnectBlobTile((blobTileUid, blobTileComp), blobCore, nearNode);
        ChangeBlobEntChem(blobTileUid, blobCoreComp.CurrentChemical);
        Dirty(blobTileUid, blobTileComp);

        var ev = new BlobTileTransformedEvent();
        RaiseLocalEvent(blobTileUid, ev);

        return true;
    }

    public void RemoveBlobTile(EntityUid tile, Entity<BlobCoreComponent> core)
    {
        QueueDel(tile);
        core.Comp.BlobTiles.Remove(tile);
    }

    public void RemoveTileWithReturnCost(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core)
    {
        RemoveBlobTile(target, core);

        FixedPoint2 returnCost = 0;
        if (target.Comp.Refudable)
        {
            returnCost = ProtoMan.Index(target.Comp.TilePrototype).Cost;
        }

        if (returnCost <= 0)
            return;

        BlobCore.ChangeBlobPoint(core, returnCost);

        _popup.PopupCoordinates(Loc.GetString("blob-get-resource", ("point", returnCost)),
            Transform(target).Coordinates,
            core.Owner,
            PopupType.Large);
    }

    public void ConnectBlobTile(
        Entity<BlobTileComponent> tile,
        Entity<BlobCoreComponent> core,
        Entity<BlobNodeComponent>? node)
    {
        var coreComp = core.Comp;
        var tileComp = tile.Comp;

        coreComp.BlobTiles.Add(tile);
        tileComp.Core = core;
        Dirty(tile, tileComp);

        var tileProto = ProtoMan.Index(tile.Comp.TilePrototype);
        if (node == null
            || !tileProto.IsSpecial)
            return;

        node.Value.Comp.PlacedSpecials.Add(tile.Comp.TilePrototype, tile.Owner);
    }

    public void SwapSpecials(Entity<BlobNodeComponent> from, Entity<BlobNodeComponent> to)
    {
        (from.Comp.PlacedSpecials, to.Comp.PlacedSpecials) = (to.Comp.PlacedSpecials, from.Comp.PlacedSpecials);
        Dirty(from);
        Dirty(to);
    }

    private static readonly EntProtoId AttackAnimation = "WeaponArcPunch";

    public void DoLunge(EntityUid from, EntityUid target)
    {
        var xform = Transform(from);
        var targetPos = _transform.GetWorldPosition(target);
        var localPos = Vector2.Transform(targetPos, _transform.GetInvWorldMatrix(xform));
        localPos = xform.LocalRotation.RotateVec(localPos);

        _meleeWeaponSystem.DoLunge(from, from, Angle.Zero, localPos, AttackAnimation, Angle.Zero, false);
    }

    public void ChangeBlobEntChem(EntityUid uid, ProtoId<BlobChemPrototype> newChem)
    {
        var color = ProtoMan.Index(newChem).Color;
        _appearance.SetData(uid, BlobColorVisuals.Color, color);
    }

    public void KillBlobTile(Entity<BlobTileComponent> ent)
    {
        ent.Comp.Core = null;
        _appearance.SetData(ent.Owner, BlobColorVisuals.Color, Color.White);
    }

    private void TryUpgrade(
        Entity<BlobTileComponent, BlobUpgradeableTileComponent> target,
        EntityUid core)
    {
        var ev = new BlobTransformTileActionEvent(
            performer: core,
            target: Transform(target).Coordinates,
            transformFrom: target.Comp1.TilePrototype,
            tileType: target.Comp2.TransformTo);

        RaiseLocalEvent(core, ev);
    }

    private void AddUpgradeVerb(EntityUid uid, BlobUpgradeableTileComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!CoreQuery.HasComp(args.User)
            ||!TileQuery.TryComp(uid, out var tileComp)
            || tileComp.Core == null)
            return;

        var verbName = Loc.GetString(comp.Locale);

        AlternativeVerb verb = new()
        {
            Act = () => TryUpgrade((uid, tileComp, comp), args.User),
            Text = verbName,
        };
        args.Verbs.Add(verb);
    }

    private static readonly ProtoId<NpcFactionPrototype> BlobFaction = "Blob";

    private void OnMapInit(Entity<BlobTileComponent> ent, ref MapInitEvent args)
    {
        var faction = EnsureComp<NpcFactionMemberComponent>(ent);
        Entity<NpcFactionMemberComponent?> factionEnt = (ent, faction);

        _npcFactionSystem.ClearFactions(factionEnt, false);
        _npcFactionSystem.AddFaction(factionEnt, BlobFaction);

        // make alive - true for npc combat
        EnsureComp<MobStateComponent>(ent);
        EnsureComp<MobThresholdsComponent>(ent);
    }

    private void OnTerminate(Entity<BlobTileComponent> ent, ref EntityTerminatingEvent args)
    {
        if (!CoreQuery.TryComp(ent.Comp.Core, out var coreComp))
            return;

        coreComp.BlobTiles.Remove(ent.Owner);
    }
}
