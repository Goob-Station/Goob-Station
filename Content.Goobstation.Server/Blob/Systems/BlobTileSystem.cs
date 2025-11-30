using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Shared.Blob.Components;
using Content.Goobstation.Shared.Blob.Systems;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Server.Blob.Systems;

public sealed class BlobTileSystem : SharedBlobTileSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlobTileComponent, BlobTileGetPulseEvent>(OnPulsed);
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

        var core = (component.Core.Value, coreComp);

        HealTile((uid, component), core);

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

        if (clickLocation == null)
            return;

        var ev = new AfterInteractEvent(core.Value, EntityUid.Invalid, null, clickLocation.Value, true);
        BlobCore.Interact(core, ev);
        args.Handled = true;
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
            if (TileQuery.HasComp(uid))
                return false;
        }

        // Don't automatically grow in space
        var targetTile = _mapSystem.GetTileRef(grid, grid, picked);
        if (targetTile.Tile.IsEmpty)
            return false;

        position = picked;
        return true;
    }

    private void HealTile(Entity<BlobTileComponent> ent, Entity<BlobCoreComponent> core)
    {
        var healCore = new DamageSpecifier();
        var modifier = ProtoMan.Index(core.Comp.CurrentChemical).HealingModifier;

        foreach (var keyValuePair in ent.Comp.HealthOfPulse.DamageDict)
        {
            healCore.DamageDict.TryAdd(keyValuePair.Key, keyValuePair.Value * modifier);
        }

        _damageableSystem.TryChangeDamage(ent, healCore);
    }
}
