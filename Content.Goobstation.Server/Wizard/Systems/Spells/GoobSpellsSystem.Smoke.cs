
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared.Chemistry.Components;
using Content.Shared.Coordinates.Helpers;

namespace Content.Goobstation.Server.Wizard.Systems;

public sealed partial class GoobSpellsSystem
{
    protected override void OnSmokeRelay(SmokeSpellEvent ev)
    {
        base.OnSmokeRelay(ev);

        var xform = Transform(ev.Performer);
        var mapCoords = _xform.GetMapCoordinates(ev.Performer, xform);

        if (!_mapManager.TryFindGridAt(mapCoords, out var gridUid, out var grid) ||
            !_map.TryGetTileRef(gridUid, grid, xform.Coordinates, out var tileRef) ||
            tileRef.Tile.IsEmpty)
            return;

        if (_spreader.RequiresFloorToSpread(ev.Proto.ToString()) && _turf.IsSpace(tileRef.Tile))
            return;

        var coords = _map.MapToGrid(gridUid, mapCoords);
        var ent = Spawn(ev.Proto, coords.SnapToGrid());
        if (!TryComp<SmokeComponent>(ent, out var smoke))
        {
            Log.Error($"Smoke prototype {ev.Proto} was missing SmokeComponent");
            Del(ent);
            return;
        }

        _smoke.StartSmoke(ent, new Solution("ThickSmoke", 50), ev.Duration, ev.SpreadAmount, smoke);
    }
}