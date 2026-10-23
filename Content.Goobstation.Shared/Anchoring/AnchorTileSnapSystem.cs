using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Anchoring;

/// <summary>
///    Temporary fix, MGS made it so anchored entities no longer snap to the center of the tile, there wasnt a breaking
///    change for this so I assume its unintentional, and we can delete this after its fixed in RT,
///    if this is intentional we will have to go fix it everywhere manually.
public sealed partial class AnchorTileSnapSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedMapSystem _map = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityQuery<MapGridComponent> _gridQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnchorStateChangedEvent>(OnAnchorStateChanged);
    }

    private void OnAnchorStateChanged(ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored || args.Detaching || _timing.ApplyingState)
            return;

        var xform = args.Transform;
        if (!_gridQuery.TryComp(xform.ParentUid, out var grid))
            return;

        var tile = _map.TileIndicesFor(xform.ParentUid, grid, xform.Coordinates);
        var center = _map.GridTileToLocal(xform.ParentUid, grid, tile);
        if (center.Position.EqualsApprox(xform.LocalPosition))
            return;

        _transform.SetCoordinates(args.Entity, xform, center, unanchor: false);
    }
}
