// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Solutions;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids.Components;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Goobstation.Server.Chemistry;

public sealed class SpillableMeleePuddleSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;

    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpillableComponent, SpillableCreatePuddleOnHitEvent>(OnCreatePuddle);
    }

    private void OnCreatePuddle(Entity<SpillableComponent> ent, ref SpillableCreatePuddleOnHitEvent args)
    {
        if (args.Amount <= 0f || !TryComp(ent, out MeleeWeaponComponent? melee))
            return;

        if (!_solution.TryGetDrainableSolution(ent.Owner, out var soln, out _))
            return;

        var coords = _transform.ToMapCoordinates(args.Coords);
        var userCoords = _transform.GetMapCoordinates(args.User);

        if (coords.MapId != userCoords.MapId)
            return;

        var dir = coords.Position - userCoords.Position;
        var length = dir.Length();

        if (length < 0.01f)
            return;

        var range = MathF.Min(melee.Range, length);
        if (range < length)
        {
            dir *= range / length;
            coords = new MapCoordinates( userCoords.Position + dir, coords.MapId);
        }

        var splitSolution = _solution.SplitSolution(soln.Value, args.Amount);

        if (!_mapMan.TryFindGridAt(coords, out var gridUid, out var mapGrid))
            return;

        var tileRef = _map.GetTileRef(gridUid, mapGrid, coords);
        _puddle.TrySpillAt(tileRef, splitSolution, out _);
    }
}
