using Content.Server.Trigger.Systems;
using Content.Shared.EntityEffects;
using Content.Goobstation.Shared.EntityEffects;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class DoSmokeEffectSystem : EntityEffectSystem<TransformComponent, DoSmokeEffect>
{
    [Dependency] private SmokeOnTriggerSystem _smoke = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<DoSmokeEffect> args)
    {
        var e = args.Effect;
        _smoke.SpawnSmoke(ent, e.SmokePrototype, e.Solution, e.Duration, e.SpreadAmount);
    }
}
