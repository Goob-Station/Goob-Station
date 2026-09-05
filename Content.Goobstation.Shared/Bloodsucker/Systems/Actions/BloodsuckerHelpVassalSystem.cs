using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Components.Vassals;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerHelpVassalSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerHelpVassalEvent>(OnHelpVassal);
        SubscribeLocalEvent<BloodsuckerComponent, BloodsuckerHelpVassalDoAfterEvent>(OnHelpVassalDoAfter);
    }

    private void OnHelpVassal(Entity<BloodsuckerComponent> ent, ref BloodsuckerHelpVassalEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerHelpVassalComponent? comp))
            return;

        // Hard-grabbing an ex-vassal
        if (TryComp(ent.Owner, out PullerComponent? puller)
            && puller.Pulling is EntityUid pulled
            && TryComp(pulled, out GrabbableComponent? grabbable)
            && grabbable.GrabStage >= GrabStage.Hard
            && HasComp<BloodsuckerExVassalComponent>(pulled))
        {
            if (!TryUseCosts(ent, comp))
                return;

            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-help-vassal-start", ("target", pulled)),
                ent.Owner, ent.Owner, PopupType.Small);

            var doAfterArgs = new DoAfterArgs(
                EntityManager,
                ent.Owner,
                comp.ReturnDelay,
                new BloodsuckerHelpVassalDoAfterEvent { Target = GetNetEntity(pulled) },
                ent.Owner,
                pulled)
            {
                BreakOnMove = true,
                BreakOnDamage = true,
                NeedHand = false,
            };

            _doAfter.TryStartDoAfter(doAfterArgs);
            args.Handled = true;
            return;
        }

        // No grab target, spawn vampire blood bag at feet
        if (!TryComp(ent.Owner, out BloodstreamComponent? bloodstream))
            return;

        var currentBlood = bloodstream.BloodSolution is { } sol
            ? (float) sol.Comp.Solution.Volume
            : 0f;

        if (currentBlood < comp.BloodBagBloodCost)
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-help-vassal-no-blood"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        if (!TryUseCosts(ent, comp))
            return;

        _bloodstream.TryModifyBloodLevel(
            new Entity<BloodstreamComponent?>(ent.Owner, bloodstream),
            -Goobstation.Maths.FixedPoint.FixedPoint2.New(comp.BloodBagBloodCost));

        var coords = Transform(ent.Owner).Coordinates;
        var bag = Spawn(comp.BloodBagProto, coords);

        // Try to put it in their hand, drop at feet if hands are full
        if (!_hands.TryPickupAnyHand(ent.Owner, bag))
            _transform.SetCoordinates(bag, coords);

        _audio.PlayPredicted(comp.Sound, ent.Owner, ent.Owner);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-help-vassal-bag-created"),
            ent.Owner, ent.Owner, PopupType.Small);

        args.Handled = true;
    }

    private void OnHelpVassalDoAfter(Entity<BloodsuckerComponent> ent, ref BloodsuckerHelpVassalDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!TryComp(ent, out BloodsuckerHelpVassalComponent? comp))
            return;

        args.Handled = true;

        var target = GetEntity(args.Target);
        if (target == EntityUid.Invalid || !Exists(target))
            return;

        if (!HasComp<BloodsuckerExVassalComponent>(target))
        {
            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-help-vassal-fail-not-exvassal"),
                ent.Owner, ent.Owner, PopupType.SmallCaution);
            return;
        }

        RemComp<BloodsuckerExVassalComponent>(target);
        EnsureComp<BloodsuckerVassalComponent>(target);

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-help-vassal-success-others", ("target", target)),
            Loc.GetString("bloodsucker-help-vassal-success-user", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Medium);

        _audio.PlayPredicted(comp.Sound, ent.Owner, ent.Owner);  // comp not in scope here, see note
    }

    private bool TryUseCosts(Entity<BloodsuckerComponent> ent, BloodsuckerHelpVassalComponent comp)
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

[Serializable, NetSerializable]
public sealed partial class BloodsuckerHelpVassalDoAfterEvent : SimpleDoAfterEvent
{
    public NetEntity Target;
}
