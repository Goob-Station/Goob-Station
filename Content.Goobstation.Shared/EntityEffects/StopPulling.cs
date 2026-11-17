using Content.Shared.EntityEffects;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class StopPulling : EntityEffectBase<StopPulling>;

public sealed partial class StopPullingEntityEffectSystem : EntityEffectSystem<PullerComponent, StopPulling>
{
    [Dependency] private PullingSystem _pulling = default!;

    // who the FUCK wrote pull code ???????
    protected override void Effect(Entity<PullerComponent> ent, ref EntityEffectEvent<StopPulling> args)
    {
        if (!TryComp(ent.Comp.Pulling, out PullableComponent? pullableComp))
            return;

        _pulling.TryStopPull(ent.Comp.Pulling.Value, pullableComp);
    }
}
