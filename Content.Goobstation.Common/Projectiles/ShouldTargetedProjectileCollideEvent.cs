namespace Content.Goobstation.Common.Projectiles;

public sealed class ShouldTargetedProjectileCollideEvent(EntityUid target) : HandledEntityEventArgs
{
    public EntityUid Target = target;
}
