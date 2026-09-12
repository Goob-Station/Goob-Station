using Robust.Shared.Physics.Events;

namespace Content.Shared._Lavaland.Chasm.Teleport;

/// <summary>
/// Teleports you out of sea of fantasy trees.
/// </summary>

public sealed class SeaOfFantasyTreesExitSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SeaOfFantasyTreesExitComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, SeaOfFantasyTreesExitComponent comp, ref StartCollideEvent args)
    {
        var query = EntityQueryEnumerator<SeaOfFantasyTreesExitBeaconComponent, TransformComponent>();
        if (!query.MoveNext(out _, out _, out var beaconXform))
            return;

        _transform.SetCoordinates(args.OtherEntity, beaconXform.Coordinates);
    }
}
