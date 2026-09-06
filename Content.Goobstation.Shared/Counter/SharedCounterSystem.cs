using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Counter;

/// <summary>
/// Generic "counter" ability. 
/// </summary>
public sealed class SharedCounterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ArmCounterActionEvent>(OnArm);

        SubscribeLocalEvent<CounterComponent, AttackedEvent>(OnAttacked);
        SubscribeLocalEvent<CounterComponent, BeforeDamageChangedEvent>(OnBeforeDamage);
        SubscribeLocalEvent<CounterComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<CounterComponent, ProjectileReflectAttemptEvent>(OnProjectileReflect);
        SubscribeLocalEvent<CounterComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<CounterComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
    }

    private void OnArm(ArmCounterActionEvent args)
    {
        if (args.Handled || !TryComp<CounterActionComponent>(args.Action, out var cfg))
            return;

        args.Handled = true;

        var counter = EnsureComp<CounterComponent>(args.Performer);
        counter.DodgeProjectile = cfg.DodgeProjectile;
        counter.ProjectileOnly = cfg.ProjectileOnly;
        counter.MeleeOnly = cfg.MeleeOnly;
        counter.CancelOnBuildup = cfg.CancelOnBuildup;
        counter.ArmedShader = cfg.ArmedShader;
        counter.BuildupShader = cfg.BuildupShader;
        counter.TriggeredShader = cfg.TriggeredShader;
        counter.TriggerFlashTime = cfg.TriggerFlashTime;
        counter.BuildupSound = cfg.BuildupSound;
        counter.ArmedSound = cfg.ArmedSound;
        counter.ArmedLoopSound = cfg.ArmedLoopSound;
        counter.BeatSound = cfg.BeatSound;
        counter.TriggerSound = cfg.TriggerSound;
        counter.FizzleSound = cfg.FizzleSound;
        counter.SpeedModifier = cfg.SpeedModifier;
        counter.Active = false;
        counter.Triggered = false;
        counter.Ended = false;
        counter.LastBeat = -1;
        counter.ArmedStream = _audio.Stop(counter.ArmedStream);
        counter.PendingMeleeAttacker = null;
        counter.ArmTime = _timing.CurTime;
        counter.BuildupEndTime = _timing.CurTime + cfg.BuildupTime;
        counter.CounterEndTime = counter.BuildupEndTime + cfg.CounterTime;
        Dirty(args.Performer, counter);

        _movementSpeed.RefreshMovementSpeedModifiers(args.Performer);
        _audio.PlayPredicted(counter.BuildupSound, args.Performer, args.Performer);
    }

    private void OnRemove(Entity<CounterComponent> ent, ref ComponentRemove args)
    {
        ent.Comp.ArmedStream = _audio.Stop(ent.Comp.ArmedStream);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshSpeed(Entity<CounterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.LifeStage >= ComponentLifeStage.Stopping || ent.Comp.Triggered)
            return;

        args.ModifySpeed(ent.Comp.SpeedModifier);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<CounterComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Triggered)
            {
                if (now >= comp.CounterEndTime)
                    EndCounter(uid, comp);
                continue;
            }

            if (!comp.Active)
            {
                if (now < comp.BuildupEndTime)
                    continue;

                comp.Active = true;
                Dirty(uid, comp);
                _audio.PlayPredicted(comp.ArmedSound, uid, uid);
                comp.ArmedStream = _audio.PlayPredicted(comp.ArmedLoopSound, uid, uid)?.Entity ?? comp.ArmedStream;
                continue;
            }

            if (now >= comp.CounterEndTime)
                EndCounter(uid, comp, fizzle: true);
        }
    }

    #region Melee

    private void OnAttacked(Entity<CounterComponent> ent, ref AttackedEvent args)
    {
        if (ent.Comp.Triggered || ent.Comp.ProjectileOnly || ent.Owner == args.User)
            return;

        if (!ent.Comp.Active)
        {
            if (ent.Comp.CancelOnBuildup)
                EndCounter(ent, ent.Comp, fizzle: true);
            return;
        }

        ent.Comp.PendingMeleeAttacker = args.User;
        Trigger(ent, args.User);
    }

    private void OnBeforeDamage(Entity<CounterComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (ent.Comp.PendingMeleeAttacker is not { } attacker || args.Origin != attacker)
            return;

        ent.Comp.PendingMeleeAttacker = null;
        args.Cancelled = true;
    }

    #endregion

    #region Projectile

    private void OnPreventCollide(Entity<CounterComponent> ent, ref PreventCollideEvent args)
    {
        if (ent.Comp.Triggered
            || ent.Comp.MeleeOnly
            || !ent.Comp.DodgeProjectile
            || !TryComp<ProjectileComponent>(args.OtherEntity, out var proj) || !args.OtherFixture.Hard)
            return;

        if (!ent.Comp.Active)
        {
            if (ent.Comp.CancelOnBuildup)
                EndCounter(ent, ent.Comp, fizzle: true);
            return;
        }

        args.Cancelled = true;

        Trigger(ent, proj.Shooter ?? args.OtherEntity);
    }

    private void OnProjectileReflect(Entity<CounterComponent> ent, ref ProjectileReflectAttemptEvent args)
    {
        if (ent.Comp.Triggered || ent.Comp.MeleeOnly)
            return;

        if (!ent.Comp.Active)
        {
            if (ent.Comp.CancelOnBuildup)
                EndCounter(ent, ent.Comp, fizzle: true);
            return;
        }

        args.Cancelled = true;
        Trigger(ent, args.Component.Shooter ?? args.ProjUid);
    }

    #endregion

    private void Trigger(Entity<CounterComponent> ent, EntityUid attacker)
    {
        if (ent.Comp.Triggered)
            return;

        ent.Comp.Triggered = true;
        ent.Comp.BuildupEndTime = _timing.CurTime;
        ent.Comp.CounterEndTime = _timing.CurTime + ent.Comp.TriggerFlashTime;
        Dirty(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
        ent.Comp.ArmedStream = _audio.Stop(ent.Comp.ArmedStream);
        _audio.PlayPredicted(ent.Comp.TriggerSound, ent, ent);

        var ev = new CounterTriggeredEvent(attacker);
        RaiseLocalEvent(ent.Owner, ref ev);
    }

    /// <summary>
    /// Removes the counter.
    /// </summary>
    private void EndCounter(EntityUid uid, CounterComponent comp, bool fizzle = false)
    {
        if (comp.Ended)
            return;

        comp.Ended = true;
        comp.ArmedStream = _audio.Stop(comp.ArmedStream);

        if (fizzle)
            _audio.PlayPredicted(comp.FizzleSound, uid, uid);

        RemCompDeferred<CounterComponent>(uid);
    }
}
