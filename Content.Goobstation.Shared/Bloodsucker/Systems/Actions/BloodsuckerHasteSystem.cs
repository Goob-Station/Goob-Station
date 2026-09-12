using Content.Goobstation.Shared.Bloodsuckers.Components;
using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Goobstation.Shared.Bloodsuckers.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;

namespace Content.Goobstation.Shared.Bloodsuckers.Systems;

public sealed class BloodsuckerHasteSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly BloodsuckerHumanitySystem _humanity = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BloodsuckerHasteComponent, BloodsuckerHasteEvent>(OnHaste);
        SubscribeLocalEvent<BloodsuckerHasteComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<BloodsuckerHasteComponent, LandEvent>(OnLand);
        SubscribeLocalEvent<BloodsuckerHasteComponent, StopThrowEvent>(OnStopThrow);
    }

    private void OnHaste(Entity<BloodsuckerHasteComponent> ent, ref BloodsuckerHasteEvent args)
    {
        if (!HasComp<BloodsuckerComponent>(ent.Owner))
            return;

        // Can't dash while being pulled aggressively
        if (TryComp(ent.Owner, out PullableComponent? pullable) && pullable.BeingPulled)
        {
            _popup.PopupPredicted(Loc.GetString("bloodsucker-haste-fail-grabbed"),
                ent.Owner, ent.Owner, PopupType.Small);
            return;
        }

        if (!TryUseCosts(ent.Owner, ent.Comp))
            return;

        ent.Comp.IsDashing = true;
        ent.Comp.AlreadyHit.Clear();
        Dirty(ent);

        _audio.PlayPredicted(ent.Comp.DashSound, ent.Owner, ent.Owner);
        _popup.PopupPredicted(Loc.GetString("bloodsucker-haste-start"),
            ent.Owner, ent.Owner, PopupType.Small);

        // Throw toward the clicked map position
        _throwing.TryThrow(ent.Owner, args.Target, ent.Comp.DashSpeed, animated: false);
    }

    private void OnCollide(Entity<BloodsuckerHasteComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.IsDashing)
            return;

        var other = args.OtherEntity;

        // Only hit mobs
        if (!HasComp<MobStateComponent>(other))
            return;

        if (other == ent.Owner)
            return;

        if (!ent.Comp.AlreadyHit.Add(other))
            return; // already hit this entity this dash, unlikely to happen, but just in case

        var knockdown = TimeSpan.FromSeconds(
            ent.Comp.KnockdownBase + ent.Comp.ActionLevel * ent.Comp.KnockdownPerLevel);

        _stun.TryKnockdown(other, knockdown, true);
        _stun.TryAddParalyzeDuration(other, TimeSpan.FromSeconds(0.1));

        _audio.PlayPredicted(ent.Comp.HitSound, other, ent.Owner);
    }

    private void OnLand(Entity<BloodsuckerHasteComponent> ent, ref LandEvent args)
    {
        StopDash(ent);
    }

    private void OnStopThrow(Entity<BloodsuckerHasteComponent> ent, ref StopThrowEvent args)
    {
        StopDash(ent);
    }

    private void StopDash(Entity<BloodsuckerHasteComponent> ent)
    {
        if (!ent.Comp.IsDashing)
            return;

        ent.Comp.IsDashing = false;
        ent.Comp.AlreadyHit.Clear();
        Dirty(ent);
    }

    private bool TryUseCosts(EntityUid uid, BloodsuckerHasteComponent comp)
    {
        if (comp.DisabledInFrenzy && HasComp<BloodsuckerFrenzyComponent>(uid))
            return false;

        if (comp.HumanityCost != 0f && TryComp(uid, out BloodsuckerHumanityComponent? humanity))
            _humanity.ChangeHumanity(
                new Entity<BloodsuckerHumanityComponent>(uid, humanity),
                -comp.HumanityCost);

        return true;
    }
}
