using Content.Goobstation.Common.Stunnable;
using Content.Goobstation.Common.Weapons.DelayedKnockdown;
using Content.Goobstation.Shared.Clothing;
using Content.Goobstation.Shared.SpecialPassives.SuperAdrenaline.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared._Shitmed.Medical.Surgery.Consciousness.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Pain;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Components;
using Content.Shared._Shitmed.Medical.Surgery.Pain.Systems;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Alert;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.SpecialPassives.SuperAdrenaline;

public sealed class SharedSuperAdrenalineSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly ConsciousnessSystem _consciousness = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly PainSystem _pain = default!;
    [Dependency] private readonly SleepingSystem _sleep = default!;

    private EntityQuery<MobStateComponent> _mobstateQuery;
    private EntityQuery<NerveSystemComponent> _nerveQuery;
    private EntityQuery<StaminaComponent> _staminaQuery;
    private EntityQuery<SleepingComponent> _sleepingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobstateQuery = GetEntityQuery<MobStateComponent>();
        _nerveQuery = GetEntityQuery<NerveSystemComponent>();
        _staminaQuery = GetEntityQuery<StaminaComponent>();
        _sleepingQuery = GetEntityQuery<SleepingComponent>();

        SubscribeLocalEvent<SuperAdrenalineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SuperAdrenalineComponent, ComponentRemove>(OnRemoved);

        SubscribeLocalEvent<SuperAdrenalineComponent, BeforeStunEvent>(OnAttemptStun);
        SubscribeLocalEvent<SuperAdrenalineComponent, BeforeKnockdownEvent>(OnAttemptKnockdown);
        SubscribeLocalEvent<SuperAdrenalineComponent, BeforeTrySlowdownEvent>(OnAttemptTrySlowdown);
        SubscribeLocalEvent<SuperAdrenalineComponent, ModifySlowOnDamageSpeedEvent>(OnDamageSlowdown);
        SubscribeLocalEvent<SuperAdrenalineComponent, TryingToSleepEvent>(OnAttemptSleep);
        SubscribeLocalEvent<SuperAdrenalineComponent, MobStateChangedEvent>(OnMobStateChange);

        SubscribeLocalEvent<SuperAdrenalineComponent, DelayedKnockdownAttemptEvent>(OnAttemptDelayedKnockdown);
    }

    private void OnMapInit(Entity<SuperAdrenalineComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobstateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        if (ent.Comp.IgnoreStun)
            RemComp<StunnedComponent>(ent);

        if (ent.Comp.IgnoreKnockdown)
        {
            RemComp<KnockedDownComponent>(ent);
            RemComp<DelayedKnockdownComponent>(ent);
        }

        if (ent.Comp.IgnoreSleep)
        {
            // forces you to wake up even if asleep via chems
            if (ent.Comp.IgnoreSleep && _sleepingQuery.TryComp(ent, out var sleep))
                _sleep.TryWaking((ent, sleep), true);
        }

        if (ent.Comp.IgnorePain
            && _nerveQuery.TryComp(ent, out var nerveSys))
        {
            var duration = ent.Comp.Duration;

            // ForceConscious doesn't have a permanent version so it needs a timespan unfortunately
            // the alternative is just putting in Float.MaxValue which is ehhhh
            if (duration.HasValue)
                // should force you to stay conscious regardless of pain
                _consciousness.ForceConscious(ent, TimeSpan.FromSeconds((double) duration));

            // reset any current pain to 0
            nerveSys.Pain = 0;
            Dirty(ent, nerveSys);

            // this should make it so you dont gain pain from any sources
            _pain.TryAddPainMultiplier(
                ent,
                "SuperAdrenaline",
                0f,
                PainDamageTypes.WoundPain,
                nerveSys,
                duration.HasValue ? TimeSpan.FromSeconds((double) duration) : null);
        }

        Cycle(ent);
    }

    private void OnRemoved(Entity<SuperAdrenalineComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent, (ProtoId<AlertPrototype>) ent.Comp.AlertId); // incase there was still time left on removal
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<SuperAdrenalineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime
                && comp.Duration.HasValue) // assume it lasts forever otherwise
                RemCompDeferred<SuperAdrenalineComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<SuperAdrenalineComponent> ent)
    {
        if (!TryValidMobstateCheck(ent))
            return;

        if (!_staminaQuery.TryComp(ent, out var stam))
            return;

        stam.StaminaDamage = Math.Clamp(ent.Comp.StaminaRegeneration, 0f, stam.CritThreshold);
        Dirty(ent, stam);

        if (ent.Comp.PassiveDamage != null)
            _damageable.TryChangeDamage(
                ent,
                ent.Comp.PassiveDamage,
                true,
                false,
                targetPart: TargetBodyPart.All,
                splitDamage: SplitDamageBehavior.SplitEnsureAllOrganic);
    }

    #region Event Handlers
    private void OnAttemptStun(Entity<SuperAdrenalineComponent> ent, ref BeforeStunEvent args)
    {
        args.Cancelled = ent.Comp.IgnoreStun;
    }

    private void OnAttemptKnockdown(Entity<SuperAdrenalineComponent> ent, ref BeforeKnockdownEvent args)
    {
        args.Cancelled = ent.Comp.IgnoreKnockdown;
    }

    // this won't stop slowdown from non TrySlowdown sources (such as stepping on lava)
    private void OnAttemptTrySlowdown(Entity<SuperAdrenalineComponent> ent, ref BeforeTrySlowdownEvent args)
    {
        args.Cancelled = ent.Comp.IgnoreStunSlowdown;
    }

    private void OnDamageSlowdown(Entity<SuperAdrenalineComponent> ent, ref ModifySlowOnDamageSpeedEvent args)
    {
        args.Speed = ent.Comp.IgnoreDamageSlowdown ? 1f : args.Speed;
    }

    private void OnAttemptSleep(Entity<SuperAdrenalineComponent> ent, ref TryingToSleepEvent args)
    {
        args.Cancelled = ent.Comp.IgnoreSleep;
    }

    private void OnAttemptDelayedKnockdown(Entity<SuperAdrenalineComponent> ent, ref DelayedKnockdownAttemptEvent args)
    {
        if (ent.Comp.IgnoreKnockdown)
            args.Cancel();
    }

    private void OnMobStateChange(Entity<SuperAdrenalineComponent> ent, ref MobStateChangedEvent args)
    {
        ent.Comp.Mobstate = args.NewMobState;
    }
    #endregion

    #region Helper Methods
    private bool TryValidMobstateCheck(Entity<SuperAdrenalineComponent> ent)
    {
        if (ent.Comp.Mobstate == MobState.Dead)
        {
            RemComp<SuperAdrenalineComponent>(ent);
            return false;
        }

        return true;
    }
    #endregion
}
