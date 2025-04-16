using Content.Goobstation.Shared.Factory;
using Robust.Client.GameObjects;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Factory;

/// <summary>
/// Animations robotic arm's arm layer swinging.
/// Can't be done with engine AnimationPlayer as it can't animate individual layers.
/// </summary>
public sealed class RoboticArmAnimationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void FrameUpdate(float frameTime)
    {
        var query = EntityQueryEnumerator<RoboticArmComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextMove is {} nextMove)
                Animate((uid, comp), nextMove);
            else
                Reset(uid);
        }
    }

    private void Animate(Entity<RoboticArmComponent> ent, TimeSpan nextMove)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var started = nextMove - ent.Comp.MoveDelay;
        // 0-1 unless something weird happens
        var progress = (_timing.CurTime - started) / ent.Comp.MoveDelay;
        if (!ent.Comp.HasItem) // returning to the resting position when emptied
            progress = 1f - progress;
        var angle = Angle.FromDegrees(progress * 180f);
        sprite.LayerSetRotation(RoboticArmLayers.Arm, angle);
    }

    private void Reset(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.LayerSetRotation(RoboticArmLayers.Arm, Angle.Zero);
    }
}
