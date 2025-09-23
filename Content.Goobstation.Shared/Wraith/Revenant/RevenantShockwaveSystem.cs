using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared.Random.Helpers;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Wraith.Revenant;

public sealed class RevenantShockwaveSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevenantShockwaveComponent, RevenantShockwaveEvent>(OnShockwave);
    }

    private void OnShockwave(Entity<RevenantShockwaveComponent> ent, ref RevenantShockwaveEvent args)
    {

        var grid = _transform.GetGrid(ent.Owner);
        if (!TryComp<MapGridComponent>(grid, out var map))
            return;

        //aaaa

        var tiles = _mapSystem.GetTilesIntersecting(
                grid.Value,
                map,
                Box2.CenteredAround(_transform.GetWorldPosition(ent.Owner),
                    new Vector2(ent.Comp.SearchRange * 2, ent.Comp.SearchRange)))
            .ToArray();

    }
}
