using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Components.Vassals;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Buckle.Components;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerVassalizeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerVassalizeEvent>(OnVassalize);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerVassalizeDoAfterEvent>(OnVassalizeDoAfter);
    }

    private void OnVassalize(Entity<BloodsuckerComponent> ent, ref BloodsuckerVassalizeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerVassalizeComponent? comp))
            return;

        var target = args.Target;

        if (target == EntityUid.Invalid || target == ent.Owner)
            return;

        // Already a vassal
        if (HasComp<BloodsuckerVassalComponent>(target))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-already-vassal", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Can't vassalize another bloodsucker
        if (HasComp<BloodsuckerComponent>(target))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-is-bloodsucker"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        // Target must be strapped to something (the rack)
        if (!TryComp(target, out BuckleComponent? buckle) || !buckle.Buckled)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-not-strapped", ("target", target)),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // The strap must be a vassal rack
        if (buckle.BuckledTo is not EntityUid strap
            || !HasComp<BloodsuckerVassalRackComponent>(strap))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-wrong-rack"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Target must be alive
        if (TryComp(target, out MobStateComponent? mobState)
            && mobState.CurrentState == MobState.Dead)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-dead", ("target", target)),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (!TryUseCosts(ent, comp))
            return;

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-vassalize-start-others",
                ("user", ent.Owner), ("target", target)),
            Loc.GetString("bloodsucker-vassalize-start-user", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Medium);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            comp.VassalizeDelay,
            new BloodsuckerVassalizeDoAfterEvent { Target = GetNetEntity(target) },
            ent.Owner,
            target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnVassalizeDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerVassalizeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        var target = GetEntity(args.Target);
        if (!Exists(target))
            return;

        if (!TryComp(ent, out BloodsuckerVassalizeComponent? comp))
            return;

        // Re-check they're still strapped to a rack
        if (!TryComp(target, out BuckleComponent? buckle)
            || !buckle.Buckled
            || buckle.BuckledTo is not EntityUid strap
            || !HasComp<BloodsuckerVassalRackComponent>(strap))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-vassalize-interrupted"),
                ent.Owner, ent.Owner, PopupType.MediumCaution);
            return;
        }

        // Remove ex-vassal if they were one
        RemComp<BloodsuckerExVassalComponent>(target);

        EnsureComp<BloodsuckerVassalComponent>(target);

        _audio.PlayPredicted(comp.Sound, ent.Owner, ent.Owner);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-vassalize-success-others",
                ("user", ent.Owner), ("target", target)),
            Loc.GetString("bloodsucker-vassalize-success-user", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Large);

        // Let the new vassal know
        _popup.PopupEntity(
            Loc.GetString("bloodsucker-vassalize-target-message", ("user", ent.Owner)),
            target, target, PopupType.LargeCaution);
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerVassalizeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(ent))
            return false;

        if (comp.HumanityCost != 0f && TryComp(ent, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(ent.Owner, humanity),
                -comp.HumanityCost);

        return true;
    }
}
