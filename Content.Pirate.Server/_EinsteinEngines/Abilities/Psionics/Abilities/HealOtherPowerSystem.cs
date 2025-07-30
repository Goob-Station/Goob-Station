using Robust.Shared.Player;
using Content.Server.DoAfter;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Content.Shared.Psionics.Events;
using Content.Shared.Examine;
using static Content.Shared.Examine.ExamineSystemShared;
using Robust.Shared.Timing;
using Content.Shared.Actions.Events;
using Robust.Server.Audio;
using Content.Server.Atmos.Rotting;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Abilities.Psionics;

public sealed class RevivifyPowerSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly DoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly RottingSystem _rotting = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly GlimmerSystem _glimmer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, PsionicHealOtherPowerActionEvent>(OnPowerUsed);
        SubscribeLocalEvent<PsionicComponent, DispelledEvent>(OnDispelled);
        SubscribeLocalEvent<PsionicComponent, PsionicHealOtherDoAfterEvent>(OnDoAfter);
    }


    private void OnPowerUsed(EntityUid uid, PsionicComponent component, PsionicHealOtherPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, args.Target, args.PowerName, true))
            return;

        args.ModifiedAmplification = _psionics.ModifiedAmplification(uid, component);
        args.ModifiedDampening = _psionics.ModifiedDampening(uid, component);

        if (!args.Immediate)
            AttemptDoAfter(uid, component, args);
        else ActivatePower(uid, component, args);

        if (args.PopupText is not null
            && _glimmer.GlimmerOutput > args.GlimmerPopupThreshold * args.ModifiedDampening)
            _popupSystem.PopupEntity(Loc.GetString(args.PopupText, ("entity", uid)), uid,
                Filter.Pvs(uid).RemoveWhereAttachedEntity(entity => !_examine.InRangeUnOccluded(uid, entity, ExamineRange, null)),
                true,
                args.PopupType);

        if (args.PlaySound
            && _glimmer.GlimmerOutput > args.GlimmerSoundThreshold * args.ModifiedDampening)
            _audioSystem.PlayPvs(args.SoundUse, uid, args.AudioParams);

        // Sanitize the Glimmer inputs because otherwise the game will crash if someone makes MaxGlimmer lower than MinGlimmer.
        var minGlimmer = MathF.MinMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * args.ModifiedAmplification - args.ModifiedDampening;
        var maxGlimmer = MathF.MaxMagnitude(args.MinGlimmer, args.MaxGlimmer)
            * args.ModifiedAmplification - args.ModifiedDampening;

        _psionics.LogPowerUsed(uid, args.PowerName, minGlimmer, maxGlimmer);
        args.Handled = true;
    }

    private void AttemptDoAfter(EntityUid uid, PsionicComponent component, PsionicHealOtherPowerActionEvent args)
    {
        var ev = new PsionicHealOtherDoAfterEvent(_gameTiming.CurTime);
        if (args.HealingAmount is not null)
            ev.HealingAmount = args.HealingAmount;
        if (args.RotReduction is not null)
            ev.RotReduction = args.RotReduction.Value;

        ev.ModifiedAmplification = args.ModifiedAmplification;
        ev.ModifiedDampening = args.ModifiedDampening;
        ev.DoRevive = args.DoRevive;
        var doAfterArgs = new DoAfterArgs(EntityManager, uid, args.UseDelay, ev, uid, target: args.Target)
        {
            BreakOnMove = args.BreakOnMove,
            Hidden = _glimmer.GlimmerOutput > args.GlimmerDoAfterVisibilityThreshold * args.ModifiedDampening,
        };

        if (!_doAfterSystem.TryStartDoAfter(doAfterArgs, out var doAfterId))
            return;

        component.DoAfter = doAfterId;
    }

    private void OnDispelled(EntityUid uid, PsionicComponent component, DispelledEvent args)
    {
        if (component.DoAfter is null)
            return;

        _doAfterSystem.Cancel(component.DoAfter);
        component.DoAfter = null;
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, PsionicComponent component, PsionicHealOtherDoAfterEvent args)
    {
        // It's entirely possible for the caster to stop being Psionic(due to mindbreaking) mid cast
        if (component is null)
            return;
        component.DoAfter = null;

        // The target can also cease existing mid-cast
        // Or the DoAfter is cancelled(such as if the caster moves).
        if (args.Target is null
            || args.Cancelled)
            return;

        if (args.RotReduction is not null)
            _rotting.ReduceAccumulator(args.Target.Value, TimeSpan.FromSeconds(args.RotReduction.Value * args.ModifiedAmplification));

        if (!TryComp<DamageableComponent>(args.Target.Value, out var damageableComponent))
            return;

        if (args.HealingAmount is not null)
            _damageable.TryChangeDamage(args.Target.Value, args.HealingAmount * args.ModifiedAmplification, true, false, damageableComponent, uid);

        if (!args.DoRevive
            || _rotting.IsRotten(args.Target.Value)
            || !TryComp<MobStateComponent>(args.Target.Value, out var mob)
            || !_mobState.IsDead(args.Target.Value, mob)
            || !_mobThreshold.TryGetThresholdForState(args.Target.Value, MobState.Dead, out var threshold)
            || damageableComponent.TotalDamage > threshold)
            return;

        _mobState.ChangeMobState(args.Target.Value, MobState.Critical, mob, uid);
    }

    // This would be the same thing as OnDoAfter, except that here the target isn't nullable, so I have to reuse code with different arguments.
    private void ActivatePower(EntityUid uid, PsionicComponent component, PsionicHealOtherPowerActionEvent args)
    {
        if (component is null)
            return;

        if (args.RotReduction is not null)
            _rotting.ReduceAccumulator(args.Target, TimeSpan.FromSeconds(args.RotReduction.Value * args.ModifiedAmplification));

        if (!TryComp<DamageableComponent>(args.Target, out var damageableComponent))
            return;

        if (args.HealingAmount is not null)
            _damageable.TryChangeDamage(args.Target, args.HealingAmount * args.ModifiedAmplification, true, false, damageableComponent, uid);

        if (!args.DoRevive
            || _rotting.IsRotten(args.Target)
            || !TryComp<MobStateComponent>(args.Target, out var mob)
            || !_mobState.IsDead(args.Target, mob)
            || !_mobThreshold.TryGetThresholdForState(args.Target, MobState.Dead, out var threshold)
            || damageableComponent.TotalDamage > threshold)
            return;

        _mobState.ChangeMobState(args.Target, MobState.Critical, mob, uid);
    }
}
