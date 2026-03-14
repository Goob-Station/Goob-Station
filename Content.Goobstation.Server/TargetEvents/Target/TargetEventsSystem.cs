using Content.Goobstation.Server.Animations;
using Content.Goobstation.Shared.TargetEvents.Animations;
using Content.Goobstation.Shared.TargetEvents.StatusEffects;
using Content.Shared.StatusEffect;

namespace Content.Goobstation.Server.TargetEvents.Target;

public sealed class TargetEventsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayAnimationTargetEvent>(OnPlayAnimation);
        SubscribeLocalEvent<ApplyStatusEffectTargetEvent>(OnApplyStatusEffect);
    }

    private void OnPlayAnimation(PlayAnimationTargetEvent ev)
    {
        _animation.PlayAnimation(ev.Target, ev.AnimationID);
    }

    private void OnApplyStatusEffect(ApplyStatusEffectTargetEvent ev)
    {
        if (ev.ComponentType is null)
            return;

        _statusEffects.TryAddStatusEffect(ev.Target, ev.Key, TimeSpan.FromSeconds(ev.Time), ev.Refresh, ev.ComponentType);
    }
}
