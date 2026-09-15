// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Maps;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Oskarrr.WildLandsTribe;

/// <summary>
/// Warrior: faster on dirt/sand tiles. Pair with WildLandsWarrior tag for bush IgnoreWhitelist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WarriorTerrainComponent : Component
{
    [DataField]
    public HashSet<string> FastTiles = new()
    {
        "FloorPlanetDirt",
        "FloorDirt",
        "FloorAsteroidSand",
        "FloorAsteroidIronsand",
        "FloorDesert",
        "FloorOskarrrDryGrass",
    };

    [DataField]
    public float WalkModifier = 1.25f;

    [DataField]
    public float SprintModifier = 1.35f;

    [ViewVariables]
    public bool Active;
}

public sealed class WarriorTerrainSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarriorTerrainComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(Entity<WarriorTerrainComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.Active)
            return;

        args.ModifySpeed(ent.Comp.WalkModifier, ent.Comp.SprintModifier);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _accumulator += frameTime;
        if (_accumulator < 0.2f)
            return;
        _accumulator = 0f;

        var query = EntityQueryEnumerator<WarriorTerrainComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var warrior, out var xform))
        {
            var onFast = false;
            if (_turf.TryGetTileRef(xform.Coordinates, out var tileRef))
            {
                var tileId = _tileDef[tileRef.Value.Tile.TypeId].ID;
                onFast = warrior.FastTiles.Contains(tileId);
            }

            if (onFast == warrior.Active)
                continue;

            warrior.Active = onFast;
            Dirty(uid, warrior);
            _movement.RefreshMovementSpeedModifiers(uid);
        }
    }
}
