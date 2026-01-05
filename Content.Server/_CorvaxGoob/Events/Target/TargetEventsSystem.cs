using Content.Goobstation.Common.Effects;
using Content.Server._CorvaxGoob.Animation;
using Content.Shared._CorvaxGoob.Events.Animation;
using Content.Shared._CorvaxGoob.Events.StatusEffects;
using Content.Shared.StatusEffect;
using Robust.Server.GameObjects;

namespace Content.Server._CorvaxGoob.Events;

public sealed class TargetEventsSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SparksSystem _sparks = default!; 
    [Dependency] private readonly TransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayAnimationTargetEvent>(OnPlayAnimation);
        SubscribeLocalEvent<DoSparksTargetEvent>(OnDoSparkle);
        SubscribeLocalEvent<ApplyStatusEffectTargetEvent>(OnApplyStatusEffect);
    }

    private void OnPlayAnimation(PlayAnimationTargetEvent ev)
    {
        _animation.PlayAnimation(ev.Target, ev.AnimationID);
    }

    private void OnDoSparkle(DoSparksTargetEvent ev)
    {
        _sparks.DoSparks(_xform.GetMoverCoordinates(ev.Target), ev.MinSparks, ev.MaxSparks, ev.MinVelocity, ev.MaxVelocity, ev.PlaySound);
    }

    private void OnApplyStatusEffect(ApplyStatusEffectTargetEvent ev)
    {
        if (ev.ComponentType is null)
            return;

        _statusEffects.TryAddStatusEffect(ev.Target, ev.Key, TimeSpan.FromSeconds(ev.Time), ev.Refresh, ev.ComponentType);
    }
}
