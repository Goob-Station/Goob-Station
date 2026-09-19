using Content.Goobstation.Common.Grab;
using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Goobstation.Shared.GrabIntent;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerLungeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly GrabIntentSystem _grab = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerLungeComponent, BloodsuckerLungeEvent>(OnLunge);
        SubscribeLocalEvent<BloodsuckerLungeComponent, BloodsuckerLungeDoAfterEvent>(OnLungeDoAfter);
        SubscribeLocalEvent<BloodsuckerLungeComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<BloodsuckerLungeComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnLunge(Entity<BloodsuckerLungeComponent> ent, ref BloodsuckerLungeEvent args)
    {
        if (!TryComp(ent, out BloodsuckerComponent? vampire))
            return;

        if (args.Target == EntityUid.Invalid || args.Target == ent.Owner)
            return;

        // Can't lunge while already grabbing or being grabbed aggressively
        if (TryComp(ent.Owner, out Content.Shared.Movement.Pulling.Components.PullerComponent? puller)
            && puller.Pulling != null)
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-lunge-fail-grabbing"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (TryComp(ent.Owner, out Content.Shared.Movement.Pulling.Components.PullableComponent? pullable)
            && pullable.BeingPulled)
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-lunge-fail-grabbed"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (!TryUseCosts(ent.Owner, ent.Comp))
            return;

        ent.Comp.CurrentTarget = args.Target;

        // Level 4+ skips the wind-up entirely
        var delay = ent.Comp.ActionLevel >= 4 ? 0f : ent.Comp.StartDelay;

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            ent.Owner,
            delay,
            new BloodsuckerLungeDoAfterEvent(),
            ent.Owner,
            args.Target)
        {
            BreakOnMove = false,
            BreakOnDamage = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);

        if (ent.Comp.ActionLevel < 4)
            _popup.PopupPredicted(Loc.GetString("bloodsucker-lunge-windup"),
                ent.Owner, ent.Owner, PopupType.Small);
    }

    private void OnLungeDoAfter(Entity<BloodsuckerLungeComponent> ent, ref BloodsuckerLungeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        args.Handled = true;

        var target = ent.Comp.CurrentTarget;
        if (target == EntityUid.Invalid || !Exists(target))
            return;

        // if dead body rip heart instead of grapple
        if (TryComp(target, out MobStateComponent? mobState) && mobState.CurrentState == MobState.Dead)
        {
            RipHeart(ent.Owner, target);
            return;
        }

        // Throw the vampire toward the target
        var vampPos = _transform.GetWorldPosition(ent.Owner);
        var targetPos = _transform.GetWorldPosition(target);
        var delta = targetPos - vampPos;

        if (delta.LengthSquared() < 0.01f)
        {
            // Already adjacent, skip the throw and resolve immediately
            LungeEnd(ent, target);
            return;
        }

        _audio.PlayPredicted(ent.Comp.LungeSound, ent.Owner, ent.Owner);

        // Throw toward target
        var targetCoords = _transform.GetMoverCoordinates(target);
        _throwing.TryThrow(ent.Owner, targetCoords, ent.Comp.DashSpeed, animated: false);
    }

    private void OnCollide(Entity<BloodsuckerLungeComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsLeaping)
            return;

        var target = ent.Comp.CurrentTarget;

        // Only care about colliding with our actual target
        if (args.OtherEntity != target)
            return;

        ent.Comp.IsLeaping = false;
        Dirty(ent);

        LungeEnd(ent, target);
    }

    private void OnStopThrow(Entity<BloodsuckerLungeComponent> ent, ref StopThrowEvent args)
    {
        if (!ent.Comp.IsLeaping)
            return;

        ent.Comp.IsLeaping = false;
        Dirty(ent);

        // Landed without hitting the target, try to resolve anyway if adjacent
        var target = ent.Comp.CurrentTarget;
        if (target == EntityUid.Invalid || !Exists(target))
            return;

        if (IsAdjacent(ent.Owner, target))
            LungeEnd(ent, target);
    }

    private void LungeEnd(Entity<BloodsuckerLungeComponent> ent, EntityUid target)
    {
        if (!TryComp(target, out MobStateComponent? mobState))
            return;

        // Shouldn't happen (dead case handled earlier) but guard anyway
        if (mobState.CurrentState == MobState.Dead)
        {
            RipHeart(ent.Owner, target);
            return;
        }

        _popup.PopupPredicted(
            Loc.GetString("bloodsucker-lunge-success-others", ("user", ent.Owner), ("target", target)),
            Loc.GetString("bloodsucker-lunge-success-user", ("target", target)),
            ent.Owner, ent.Owner, PopupType.Medium);

        // Grab at hard stage
        _grab.TryGrab(ent.Owner, target, true, GrabStage.Hard);

        // Knockdown if hitting from behind
        // TO DO: Add extra check if jumping from darkness or while invisible.
        if (!IsFacingUs(target, ent.Owner))
        {
            var knockdown = TimeSpan.FromSeconds(
                ent.Comp.KnockdownBase + ent.Comp.ActionLevel * ent.Comp.KnockdownPerLevel);
            _stun.TryKnockdown(target, knockdown, true);
            _stun.TryAddParalyzeDuration(target, TimeSpan.FromSeconds(0.1));
        }
    }

    private void RipHeart(EntityUid vampire, EntityUid corpse)
    {
        if (!TryComp(corpse, out BodyComponent? body))
            return;

        foreach (var (organ, _) in _body.GetBodyOrgans(corpse, body))
        {
            var meta = MetaData(organ);
            if (!meta.EntityPrototype?.ID.ToLowerInvariant().Contains("heart") ?? true)
                continue;

            _body.RemoveOrgan(organ);

            if (TryComp(corpse, out TransformComponent? xform))
                _transform.SetWorldPosition(organ, xform.WorldPosition);

            _popup.PopupPredicted(
                Loc.GetString("bloodsucker-lunge-heart-others", ("user", vampire), ("target", corpse)),
                Loc.GetString("bloodsucker-lunge-heart-user", ("target", corpse)),
                vampire, vampire, PopupType.Large);
            break;
        }
    }

    private bool TryUseCosts(EntityUid uid, BloodsuckerLungeComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(uid))
            return false;

        if (comp.HumanityCost != 0f && TryComp(uid, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(uid, humanity),
                -comp.HumanityCost);

        return true;
    }

    /// <summary>
    /// Returns true if B is facing toward A.
    /// </summary>
    private bool IsFacingUs(EntityUid b, EntityUid a)
    {
        if (!TryComp(b, out TransformComponent? tb) || !TryComp(a, out TransformComponent? ta))
            return false;

        var delta = ta.WorldPosition - tb.WorldPosition;
        if (delta.LengthSquared() < 0.01f)
            return true;

        var facing = tb.WorldRotation.ToWorldVec();
        return System.Numerics.Vector2.Dot(facing, System.Numerics.Vector2.Normalize(delta)) > 0f;
    }

    private bool IsAdjacent(EntityUid a, EntityUid b)
    {
        if (!TryComp(a, out TransformComponent? ta) || !TryComp(b, out TransformComponent? tb))
            return false;
        var delta = ta.WorldPosition - tb.WorldPosition;
        return delta.LengthSquared() <= 2.25f; // ~1.5 tile radius
    }
}
