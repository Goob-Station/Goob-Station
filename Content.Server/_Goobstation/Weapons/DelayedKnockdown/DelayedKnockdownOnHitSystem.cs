using Content.Server.Stunnable;
using Content.Shared.Damage.Events;
using Content.Shared.StatusEffect;
using Content.Shared.Timing;

namespace Content.Server._Goobstation.Weapons.DelayedKnockdown;

public sealed class DelayedKnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DelayedKnockdownOnHitComponent, StaminaDamageMeleeHitEvent>(OnHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DelayedKnockdownComponent, StatusEffectsComponent>();
        while (query.MoveNext(out var uid, out var comp, out var status))
        {
            comp.Time -= frameTime;

            if (comp.Time > 0)
                continue;

            _stun.TryKnockdown(uid, TimeSpan.FromSeconds(comp.KnockdownTime), comp.Refresh, status);

            RemCompDeferred(uid, comp);
        }
    }

    private void OnHit(Entity<DelayedKnockdownOnHitComponent> ent, ref StaminaDamageMeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return;

        var (uid, comp) = ent;

        if (!comp.ApplyOnHeavyAttack && args.Direction != null)
            return;

        if (TryComp(uid, out UseDelayComponent? delay))
            _delay.TryResetDelay((uid, delay), id: comp.UseDelay);

        foreach (var (hit, _) in args.HitEntities)
        {
            if (!TryComp(hit, out StatusEffectsComponent? status) ||
                !_status.CanApplyEffect(hit, "KnockedDown", status) ||
                _status.HasStatusEffect(hit, "KnockedDown", status))
                continue;

            var ev = new DelayedKnockdownAttemptEvent();
            RaiseLocalEvent(hit, ev);
            if (ev.Cancelled)
                continue;

            var delayedKnockdown = EnsureComp<DelayedKnockdownComponent>(hit);
            delayedKnockdown.Time = MathF.Min(comp.Delay, delayedKnockdown.Time);
            delayedKnockdown.KnockdownTime = MathF.Max(comp.KnockdownTime, delayedKnockdown.KnockdownTime);
            delayedKnockdown.Refresh &= comp.Refresh;
        }
    }
}
