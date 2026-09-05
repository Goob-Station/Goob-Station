using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wizard.Events;
using Content.Shared._Goobstation.Wizard.Traps;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.Wizard.Systems.Spells;

public abstract partial class SharedSpellsSystem
{
    private void OnTraps(TrapsSpellEvent ev)
    {
        if (ev.Handled || !_magic.PassesSpellPrerequisites(ev.Action, ev.Performer))
            return;

        if (ev.Traps.Count == 0)
            return;

        if (_net.IsClient)
        {
            ev.Handled = true;
            return;
        }

        if (!_mind.TryGetMind(ev.Performer, out var mind, out _))
            return;

        var range = ev.Range;
        var mapPos = _xform.GetMapCoordinates(ev.Performer);
        var box = Box2.CenteredAround(mapPos.Position, new Vector2(range, range));
        var circle = new Circle(mapPos.Position, range);
        var grids = new List<Entity<MapGridComponent>>();
        _mapManager.FindGridsIntersecting(mapPos.MapId, box, ref grids);

        bool IsTileValid((EntityCoordinates, TileRef) data)
        {
            var (coords, tile) = data;

            if (_turf.IsSpace(tile))
                return false;

            var trapQuery = GetEntityQuery<WizardTrapComponent>();
            var flags = LookupFlags.Static | LookupFlags.Sundries | LookupFlags.Sensors;
            foreach (var (entity, fix) in _lookup.GetEntitiesInRange<FixturesComponent>(coords, 0.1f, flags))
            {
                if (fix.Fixtures.Any(x =>
                        x.Value.Hard && (x.Value.CollisionLayer & (int) CollisionGroup.LowImpassable) != 0))
                    return false;

                if (trapQuery.HasComp(entity))
                    return false;
            }

            return true;
        }

        var tiles = new List<(EntityCoordinates, TileRef)>();
        foreach (var grid in grids)
        {
            tiles.AddRange(_map.GetTilesIntersecting(grid.Owner, grid.Comp, circle)
                .Select(x => (_map.GridTileToLocal(grid.Owner, grid.Comp, x.GridIndices), x))
                .Where(IsTileValid));
        }

        for (var i = 0; i < Math.Min(tiles.Count, ev.Amount); i++)
        {
            var (coords, _) = _random.PickAndTake(tiles);
            var trap = Spawn(_random.Pick(ev.Traps), coords);
            var trapComp = EnsureComp<WizardTrapComponent>(trap);
            trapComp.IgnoredMinds.Add(mind);
            Dirty(trap, trapComp);
        }

        ev.Handled = true;
    }
}