using Content.Goobstation.Common.Religion;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Audio.Systems;
using System.Numerics;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerMesmerizeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerMesmerizeEvent>(OnMesmerize);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerMesmerizeDoAfterEvent>(OnMesmerizeDoAfter);
    }

    private void OnMesmerize(Entity<BloodsuckerComponent> ent, ref BloodsuckerMesmerizeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerMesmerizeComponent? comp))
            return;

        var target = args.Target;

        if (target == EntityUid.Invalid || target == ent.Owner)
            return;

        // Target must be alive and conscious
        if (!TryComp(target, out MobStateComponent? mobState)
            || mobState.CurrentState != MobState.Alive)
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-unconscious", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Bloodsuckers are immune
        if (HasComp<BloodsuckerComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-bloodsucker"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Vampire eyes covered? (waived at level 3+)
        if (comp.ActionLevel < 3 && IsEyesCovered(ent.Owner))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-eyes-covered"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Vampire or target blind?
        if (HasComp<PermanentBlindnessComponent>(ent.Owner))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-blind-self"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (HasComp<PermanentBlindnessComponent>(target))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-blind-target", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Facing checks (level 5 waives the target-must-face-you requirement)
        if (!IsFacing(ent.Owner, target))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-not-facing", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (comp.ActionLevel < 5 && !IsFacing(target, ent.Owner))
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-mesmerize-fail-target-not-facing", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-mesmerize-starting", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Small);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.StartDelay,
            new BloodsuckerMesmerizeDoAfterEvent(),
            ent.Owner,
            target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnMesmerizeDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerMesmerizeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (args.Target is not EntityUid target)
            return;

        if (!TryComp(ent, out BloodsuckerMesmerizeComponent? comp))
            return;

        args.Handled = true;

        // for Chaplain warn them, fail silently for vamp
        if (HasComp<BibleUserComponent>(target))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-mesmerize-fail-chaplain"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-mesmerize-chaplain-resist"),
                target, target, PopupType.MediumCaution);
            return;
        }

        // Already mesmerized
        if (_status.HasStatusEffect(target, "Muted"))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-mesmerize-fail-already", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Costs only consumed on success
        if (!TryUseCosts(ent, comp))
            return;

        var duration = TimeSpan.FromSeconds(comp.ParalyzeBase + comp.ActionLevel * comp.ParalyzePerLevel);

        _stun.TryAddParalyzeDuration(target, duration);

        // Mute at level 1+
        if (comp.ActionLevel >= 1)
            _status.TryAddStatusEffect(target, "Muted", out _, duration);

        _audio.PlayPredicted(comp.Sound, ent.Owner, ent.Owner);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-mesmerize-success", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Medium);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerMesmerizeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        return true;
    }

    // Checks if entity A is facing roughly toward entity B using their world rotation.
    private bool IsFacing(EntityUid a, EntityUid b)
    {
        if (!TryComp(a, out TransformComponent? ta) || !TryComp(b, out TransformComponent? tb))
            return false;

        var delta = tb.WorldPosition - ta.WorldPosition;
        if (delta.LengthSquared() < 0.01f)
            return true;

        var angle = ta.WorldRotation;
        var dir = angle.ToWorldVec();

        // Dot product > 0 means roughly facing
        return Vector2.Dot(dir, Vector2.Normalize(delta)) > 0f;
    }

    // Basically anything that might block the vamp's eyes.
    // This might have some false positives but I'm not adding a YAML list of every single item that blocks vision.
    private bool IsEyesCovered(EntityUid uid)
    {
        return HasComp<BlindfoldComponent>(uid) ||
               HasComp<EyeProtectionComponent>(uid) ||
               HasComp<IdentityBlockerComponent>(uid);
    }
}
