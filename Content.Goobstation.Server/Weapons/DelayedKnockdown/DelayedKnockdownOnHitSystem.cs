using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Goobstation.Shared.Clothing;
using Content.Server.Stunnable;
using Content.Shared.Armor;
using Content.Shared.Damage.Events;
using Content.Shared.Inventory;
using Content.Shared.StatusEffect;
using Content.Shared.Timing;

namespace Content.Goobstation.Server.Weapons.DelayedKnockdown;

public sealed class DelayedKnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly UseDelaySystem _delay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DelayedKnockdownOnHitComponent, StaminaDamageMeleeHitEvent>(OnHit);

        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, DelayedKnockdownAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, InventoryRelayedEvent<DelayedKnockdownAttemptEvent>>(
            OnInventoryAttempt);
        SubscribeLocalEvent<ModifyDelayedKnockdownComponent, ArmorExamineEvent>(OnExamine);
    }

    private void OnExamine(Entity<ModifyDelayedKnockdownComponent> ent, ref ArmorExamineEvent args)
    {
        var comp = ent.Comp;

        if (comp.Cancel)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-cancel-delayed-knockdown"));
            return;
        }

        if (comp.DelayDelta != 0f)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-modify-delayed-knockdown-delay",
                ("amount", MathF.Abs(comp.DelayDelta)),
                ("deltasign", MathF.Sign(comp.DelayDelta))));
        }

        if (comp.KnockdownTimeDelta != 0f)
        {
            args.Msg.PushNewline();
            args.Msg.AddMarkupOrThrow(Loc.GetString("armor-examine-modify-delayed-knockdown-time",
                ("amount", MathF.Abs(comp.KnockdownTimeDelta)),
                ("deltasign", MathF.Sign(comp.KnockdownTimeDelta))));
        }
    }

    private void OnInventoryAttempt(Entity<ModifyDelayedKnockdownComponent> ent,
        ref InventoryRelayedEvent<DelayedKnockdownAttemptEvent> args)
    {
        OnAttempt(ent, ref args.Args);
    }

    private void OnAttempt(Entity<ModifyDelayedKnockdownComponent> ent, ref DelayedKnockdownAttemptEvent args)
    {
        var comp = ent.Comp;

        if (comp.Cancel)
        {
            args.Cancel();
            return;
        }

        args.DelayDelta += comp.DelayDelta;
        args.KnockdownTimeDelta += comp.KnockdownTimeDelta;
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
            if (!_status.CanApplyEffect(hit, "KnockedDown"))
                continue;

            var ev = new DelayedKnockdownAttemptEvent();
            RaiseLocalEvent(hit, ev);
            if (ev.Cancelled)
                continue;

            var delayedKnockdown = EnsureComp<DelayedKnockdownComponent>(hit);
            delayedKnockdown.Time = MathF.Min(comp.Delay + ev.DelayDelta, delayedKnockdown.Time);
            delayedKnockdown.KnockdownTime =
                MathF.Max(comp.KnockdownTime + ev.KnockdownTimeDelta, delayedKnockdown.KnockdownTime);
            delayedKnockdown.Refresh &= comp.Refresh;
        }
    }
}
