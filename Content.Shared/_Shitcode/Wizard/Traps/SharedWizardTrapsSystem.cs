using Content.Shared._Goobstation.Wizard.FadingTimedDespawn;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Electrocution;
using Content.Shared.Examine;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;
using Content.Shared.Whitelist;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Wizard.Traps;

public abstract class SharedWizardTrapsSystem : EntitySystem
{
    [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;
    [Dependency] private   readonly SharedTransformSystem _transform = default!;
    [Dependency] private   readonly SharedPopupSystem _popup = default!;
    [Dependency] private   readonly SharedMindSystem _mind = default!;
    [Dependency] private   readonly SparksSystem _spark = default!;
    [Dependency] private   readonly SharedElectrocutionSystem _electrocution = default!;
    [Dependency] private   readonly SharedStunSystem _stun = default!;
    [Dependency] private   readonly StatusEffectsSystem _status = default!;
    [Dependency] private   readonly DamageableSystem _damageable = default!;
    [Dependency] private   readonly SharedAudioSystem _audio = default!;
    [Dependency] private   readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private   readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WizardTrapComponent, ExamineAttemptEvent>(OnExamineAttempt);
        SubscribeLocalEvent<WizardTrapComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<WizardTrapComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<WizardTrapComponent, StartCollideEvent>(OnStartCollide);

        SubscribeLocalEvent<StunTrapComponent, TrapTriggeredEvent>(OnStunTriggered);
        SubscribeLocalEvent<ChillTrapComponent, TrapTriggeredEvent>(OnChillTriggered);
        SubscribeLocalEvent<BlindingTrapComponent, TrapTriggeredEvent>(OnBlindTriggered);
        SubscribeLocalEvent<DamageTrapComponent, TrapTriggeredEvent>(OnDamageTriggered);
    }

    private void OnDamageTriggered(Entity<DamageTrapComponent> ent, ref TrapTriggeredEvent args)
    {
        _damageable.TryChangeDamage(args.Victim, ent.Comp.Damage, true, targetPart: TargetBodyPart.Feet);
        if (_net.IsServer && ent.Comp.SpawnedEntity is { } toSpawn)
            Spawn(toSpawn, _transform.GetMapCoordinates(ent));
    }

    private void OnBlindTriggered(Entity<BlindingTrapComponent> ent, ref TrapTriggeredEvent args)
    {
        var (_, comp) = ent;

        if (!TryComp(args.Victim, out StatusEffectsComponent? status))
            return;

        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(args.Victim,
            "TemporaryBlindness",
            comp.BlindDuration,
            true,
            status);

        _status.TryAddStatusEffect<BlurryVisionComponent>(args.Victim,
            "BlurryVision",
            comp.BlurDuration,
            true,
            status);
    }

    private void OnChillTriggered(Entity<ChillTrapComponent> ent, ref TrapTriggeredEvent args)
    {
        EnsureComp<IceCubeComponent>(args.Victim);
    }

    private void OnStunTriggered(Entity<StunTrapComponent> ent, ref TrapTriggeredEvent args)
    {
        var (uid, comp) = ent;
        var victim = args.Victim;

        _electrocution.TryDoElectrocution(victim, uid, comp.Damage, comp.StunTime, true, ignoreInsulation: true);
    }

    private void OnStartCollide(Entity<WizardTrapComponent> ent, ref StartCollideEvent args)
    {
        var (uid, comp) = ent;

        if (comp.Triggered)
            return;

        if (HasComp<GodmodeComponent>(args.OtherEntity) || HasComp<IceCubeComponent>(args.OtherEntity))
            return;

        if (IsEntityMindIgnored(args.OtherEntity, comp))
            return;

        if (!comp.Silent)
        {
            _popup.PopupClient(Loc.GetString("trap-triggered-message", ("trap", uid)),
                args.OtherEntity,
                PopupType.LargeCaution);
        }

        comp.Triggered = true;
        comp.Charges--;
        Dirty(ent);

        if (HasComp<FadingTimedDespawnComponent>(uid))
            return;

        if (comp.StunTime > TimeSpan.Zero)
            _stun.TryParalyze(args.OtherEntity, comp.StunTime, true);

        RaiseLocalEvent(uid, new TrapTriggeredEvent(args.OtherEntity));

        if (comp.Sparks)
        {
            _spark.DoSparks(Transform(uid).Coordinates,
                comp.MinSparks,
                comp.MaxSparks,
                comp.MinVelocity,
                comp.MaxVelocity,
                comp.TriggerSound == null);
        }

        _audio.PlayPredicted(comp.TriggerSound, args.OtherEntity, args.OtherEntity);

        if (_net.IsClient)
            return;

        if (comp.Effect != null)
            Spawn(comp.Effect.Value, _transform.GetMapCoordinates(uid));

        if (comp.Charges <= 0)
        {
            QueueDel(uid);
            return;
        }

        Timer.Spawn(comp.TimeBetweenTriggers,
            () =>
            {
                if (!TryComp(uid, out WizardTrapComponent? trap))
                    return;

                trap.Triggered = false;
                Dirty(uid, trap);
            });
    }

    private void OnPreventCollide(Entity<WizardTrapComponent> ent, ref PreventCollideEvent args)
    {
        if (IsEntityMindIgnored(args.OtherEntity, ent.Comp))
            args.Cancelled = true;
    }

    private void OnExamine(Entity<WizardTrapComponent> ent, ref ExaminedEvent args)
    {
        var (uid, comp) = ent;

        if (!comp.CanReveal)
            return;

        if (TerminatingOrDeleted(uid))
            return;

        if (HasComp<FadingTimedDespawnComponent>(uid))
            return;

        if (IsEntityMindIgnored(args.Examiner, comp))
            return;

        if (!_transform.InRange(uid, args.Examiner, comp.ExamineRange))
            return;

        _popup.PopupClient(Loc.GetString("trap-revealed-message", ("trap", uid)), args.Examiner, PopupType.Medium);
        if (_net.IsServer)
            _popup.PopupEntity(Loc.GetString("trap-flare-message", ("trap", uid)), uid, PopupType.MediumCaution);

        Appearance.SetData(uid, TrapVisuals.Alpha, 0.8f);

        var fading = EnsureComp<FadingTimedDespawnComponent>(uid);
        fading.Lifetime = 0.5f;
        fading.FadeOutTime = 1f;
        Dirty(uid, fading);
    }

    private void OnExamineAttempt(Entity<WizardTrapComponent> ent, ref ExamineAttemptEvent args)
    {
        var (uid, comp) = ent;

        if (TerminatingOrDeleted(uid))
            return;

        if (IsEntityMindIgnored(args.Examiner, comp))
            return;

        if (!comp.CanReveal)
            args.Cancel();
        else if (HasComp<TemporaryBlindnessComponent>(args.Examiner) || HasComp<PermanentBlindnessComponent>(args.Examiner))
            args.Cancel();
        else if (!_transform.InRange(uid, args.Examiner, comp.ExamineRange))
            args.Cancel();
    }

    private bool IsEntityMindIgnored(EntityUid user, WizardTrapComponent trap)
    {
        if (HasComp<GhostComponent>(user) || HasComp<SpectralComponent>(user) || !HasComp<MobStateComponent>(user))
            return true;

        if (trap.TargetedEntityWhitelist != null && !_whitelist.IsWhitelistPass(trap.TargetedEntityWhitelist, user))
            return true;

        if (_whitelist.IsWhitelistPass(trap.IgnoredEntityWhitelist, user))
            return true;

        return _mind.TryGetMind(user, out var mind, out _) && trap.IgnoredMinds.Contains(mind);
    }
}

public sealed class TrapTriggeredEvent(EntityUid victim) : EntityEventArgs
{
    public EntityUid Victim = victim;
}
