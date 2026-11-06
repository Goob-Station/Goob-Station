using Content.Shared.EntityEffects;
using Content.Shared.Standing;
using Content.Shared.Stunnable;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SetStandingState : EntityEffectBase<SetStandingState>
{
    [DataField(required: true)]
    public bool Standing;
}

public sealed partial class SetStandingEntityEffectSystem : EntityEffectSystem<TransformComponent, SetStandingState>
{
    [Dependency] private StandingStateSystem _standingState = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    protected override void Effect(Entity<TransformComponent> ent, ref EntityEffectEvent<SetStandingState> args)
    {
        if (!args.Effect.Standing)
            _standingState.Down(ent);
        else
            _stun.ForceStandUp(ent.Owner);
    }
}
