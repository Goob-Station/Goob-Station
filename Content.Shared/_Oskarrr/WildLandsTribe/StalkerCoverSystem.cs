// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Maps;
using Content.Shared.Stealth;
using Content.Shared.Stealth.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._Oskarrr.WildLandsTribe;

/// <summary>
/// Stalker: stealth while standing on cover tiles (tall grass / moss / bushes).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StalkerCoverComponent : Component
{
    [DataField]
    public HashSet<string> CoverTiles = new()
    {
        "FloorOskarrrMoss",
        "FloorPlanetGrass",
        "FloorGrassJungle",
        "FloorOskarrrDryGrass",
    };

    [DataField]
    public float CoverVisibility = -0.6f;

    [DataField]
    public float ExposedVisibility = 1f;

    [ViewVariables]
    public bool InCover;
}

public sealed class StalkerCoverSystem : EntitySystem
{
    [Dependency] private readonly SharedStealthSystem _stealth = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    private float _accumulator;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StalkerCoverComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StalkerCoverComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMapInit(Entity<StalkerCoverComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<StealthComponent>(ent.Owner);
    }

    private void OnShutdown(Entity<StalkerCoverComponent> ent, ref ComponentShutdown args)
    {
        RemCompDeferred<StealthComponent>(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _accumulator += frameTime;
        if (_accumulator < 0.25f)
            return;
        _accumulator = 0f;

        var query = EntityQueryEnumerator<StalkerCoverComponent, TransformComponent, StealthComponent>();
        while (query.MoveNext(out var uid, out var stalker, out var xform, out var stealth))
        {
            var inCover = IsInCover(xform, stalker);
            if (inCover == stalker.InCover)
                continue;

            stalker.InCover = inCover;
            _stealth.SetVisibility(uid, inCover ? stalker.CoverVisibility : stalker.ExposedVisibility, stealth);
            Dirty(uid, stalker);
        }
    }

    private bool IsInCover(TransformComponent xform, StalkerCoverComponent stalker)
    {
        if (!_turf.TryGetTileRef(xform.Coordinates, out var tileRef))
            return false;

        var tileId = _tileDef[tileRef.Value.Tile.TypeId].ID;
        return stalker.CoverTiles.Contains(tileId);
    }
}
