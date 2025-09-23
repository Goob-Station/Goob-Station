using Content.Goobstation.Shared.Wraith.Events;
using Content.Shared._White.Grab;

namespace Content.Goobstation.Shared.Wraith.Revenant;

/// <summary>
/// Shoves the targeted victim or object away from you, dealing damage if they collide with something.
/// </summary>
public sealed class RevenantPushSystem : EntitySystem
{
    [Dependency] private readonly GrabThrownSystem _grab = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RevenantPushComponent, RevenantPushEvent>(OnRevPush);
    }

    private void OnRevPush(Entity<RevenantPushComponent> ent, ref RevenantPushEvent args)
    {
        var entPos = _transform.GetMapCoordinates(ent).Position;
        var targetPos = _transform.GetMapCoordinates(args.Target).Position;
        var direction = targetPos - entPos;

        _grab.Throw(args.Target, ent.Owner, direction, ent.Comp.ThrowSpeed, ent.Comp.DamageWhenThrown);

        args.Handled = true;
    }
}
