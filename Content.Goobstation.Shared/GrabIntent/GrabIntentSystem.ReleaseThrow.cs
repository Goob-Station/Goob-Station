using Content.Goobstation.Common.Grab;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech;
using Content.Shared.Throwing;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.GrabIntent;

public sealed partial class GrabIntentSystem
{
    #region Release/Throw Initialization

    private void InitializeReleaseAndThrowEvents()
    {
        SubscribeLocalEvent<GrabbableComponent, UpdateCanMoveEvent>(OnGrabbedMoveAttempt);
        SubscribeLocalEvent<GrabbableComponent, SpeakAttemptEvent>(OnGrabbedSpeakAttempt);
        SubscribeLocalEvent<GrabbableComponent, GrabAttemptReleaseEvent>(OnGrabReleaseAttempt);

        SubscribeLocalEvent<GrabIntentComponent, VirtualItemThrownEvent>(OnVirtualItemThrown);
    }

    #endregion

    #region Release/Throw

    private GrabResistResult GrabRelease(Entity<GrabbableComponent?> pullable)
    {
        if (!Resolve(pullable.Owner, ref pullable.Comp, false))
            return GrabResistResult.Succeeded;

        if (_timing.CurTime < pullable.Comp.NextEscapeAttempt)
            return GrabResistResult.TooSoon;

        var seedArray = new List<int> { (int) _timing.CurTick.Value, GetNetEntity(pullable.Owner).Id };
        var seed = SharedRandomExtensions.HashCodeCombine(seedArray);
        var rand = new Random(seed);
        if (rand.Prob(pullable.Comp.GrabEscapeChance))
            return GrabResistResult.Succeeded;

        pullable.Comp.NextEscapeAttempt = _timing.CurTime.Add(TimeSpan.FromSeconds(pullable.Comp.EscapeAttemptCooldown));
        Dirty(pullable.Owner, pullable.Comp);
        return GrabResistResult.Failed;
    }

    private void OnGrabReleaseAttempt(Entity<GrabbableComponent> ent, ref GrabAttemptReleaseEvent args)
    {
        args.Released = TryGrabRelease(ent.Owner, args.user, args.puller);
    }

    private bool TryGrabRelease(EntityUid pullableUid, EntityUid? user, EntityUid pullerUid)
    {
        if (user == null || user.Value != pullableUid)
            return true;

        if (!TryComp<GrabbableComponent>(pullableUid, out var grabbable))
            return true;

        var releaseAttempt = GrabRelease((pullableUid, grabbable));
        switch (releaseAttempt)
        {
            case GrabResistResult.Failed:
                _popup.PopupPredicted(Loc.GetString("popup-grab-release-fail-self"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                return false;
            case GrabResistResult.TooSoon:
                _popup.PopupPredicted(Loc.GetString("popup-grab-release-too-soon"),
                    pullableUid,
                    pullableUid,
                    PopupType.SmallCaution);
                return false;
        }

        _popup.PopupPredicted(Loc.GetString("popup-grab-release-success-self"),
            pullableUid,
            pullableUid,
            PopupType.SmallCaution);
        _popup.PopupPredicted(
            Loc.GetString("popup-grab-release-success-puller",
                ("target", Identity.Entity(pullableUid, EntityManager))),
            pullerUid,
            pullerUid,
            PopupType.MediumCaution);
        return true;
    }

    private void OnGrabbedMoveAttempt(EntityUid uid, GrabbableComponent component, UpdateCanMoveEvent args)
    {
        if (component.GrabStage == GrabStage.No)
            return;

        args.Cancel();
    }

    private void OnGrabbedSpeakAttempt(EntityUid uid, GrabbableComponent component, SpeakAttemptEvent args)
    {
        if (component.GrabStage != GrabStage.Suffocate)
            return;

        _popup.PopupPredicted(Loc.GetString("popup-grabbed-cant-speak"), uid, uid, PopupType.MediumCaution);
        args.Cancel();
    }

    private void OnVirtualItemThrown(EntityUid uid, GrabIntentComponent component, ref VirtualItemThrownEvent args)
    {
        if (!TryComp<PullerComponent>(uid, out var puller)
            || puller.Pulling == null
            || puller.Pulling != args.BlockingEntity)
            return;

        ThrowGrabbedEntity(uid, args.Direction);
    }

    #endregion
}
