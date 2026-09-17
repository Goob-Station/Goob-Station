using Content.Server.Fluids.EntitySystems;
using Content.Server.Spreader;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Maps;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Effects;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server.Trigger.Systems;

/// <summary>
/// Handles creating smoke when <see cref="SmokeOnTriggerComponent"/> is triggered.
/// </summary>
public sealed class SmokeOnTriggerSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SpreaderSystem _spreader = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmokeOnTriggerComponent, TriggerEvent>(OnTrigger);
    }

    private void OnTrigger(Entity<SmokeOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        // TODO: move all of this into an API function in SmokeSystem

        args.Handled = true;
    }

    /// Trauma - Moved it to helper function
    /// <summary>
    /// Spawn smoke
    /// TODO This should have moved to <see cref="SmokeSystem"/>
    /// </summary>
    /// <returns></returns>
    public bool SpawnSmoke(EntityUid target, string prototype, Solution solution, TimeSpan duration, int spreadAmount)
    {
        var xform = Transform(target);
        var mapCoords = _transform.GetMapCoordinates(target, xform);
        if (!_mapMan.TryFindGridAt(mapCoords, out var gridUid, out var gridComp) ||
            !_map.TryGetTileRef(gridUid, gridComp, xform.Coordinates, out var tileRef) ||
            tileRef.Tile.IsEmpty)
        {
            return false;
        }

        if (_spreader.RequiresFloorToSpread(prototype) && _turf.IsSpace(tileRef))
            return false;

        var coords = _map.MapToGrid(gridUid, mapCoords);
        var smoke = Spawn(prototype, coords.SnapToGrid());
        if (!TryComp<SmokeComponent>(smoke, out var smokeComp))
        {
            Log.Error($"Smoke prototype {prototype} was missing SmokeComponent");
            Del(smoke);
            return false;
        }

        _smoke.StartSmoke(smoke, solution, (float) duration.TotalSeconds, spreadAmount, smokeComp);
        return true;
    }
}
