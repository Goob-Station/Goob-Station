using Robust.Shared.Physics.Events;

namespace Content.Server.WhiteDream.BloodCult.BloodBoilProjectile;

public sealed class BloodBoilProjectileSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodBoilProjectileComponent, PreventCollideEvent>(CheckCollision);
    }

    private void CheckCollision(Entity<BloodBoilProjectileComponent> ent, ref PreventCollideEvent args)
    {
        if (args.OtherEntity != ent.Comp.Target)
            args.Cancelled = true;
    }
}
