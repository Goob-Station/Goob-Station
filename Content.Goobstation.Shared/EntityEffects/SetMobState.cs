using Content.Shared.EntityEffects;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Standing;

namespace Content.Goobstation.Shared.EntityEffects;

public sealed partial class SetMobState : EntityEffectBase<SetMobState>
{
    [DataField(required: true)]
    public MobState State;
};

/// <summary>
/// Chances are you shouldnt use this. this is for admin-fu instant death
/// </summary>
public sealed partial class SetMobStateEntityEffectSystem : EntityEffectSystem<MobStateComponent, SetMobState>
{
    [Dependency] private MobStateSystem _mobState = default!;

    protected override void Effect(Entity<MobStateComponent> ent, ref EntityEffectEvent<SetMobState> args)
    {
        _mobState.ChangeMobState(ent, args.Effect.State);
    }
}
