using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Physics.Components;

namespace Content.Goobstation.Shared.MartialArts;

public abstract partial class SharedMartialArtsSystem
{
    private void InitializeCapoeira()
    {
        SubscribeLocalEvent<CanPerformComboComponent, Events.PushKickPerformedEvent>(OnPushKick);
        SubscribeLocalEvent<CanPerformComboComponent, Events.CircleKickPerformedEvent>(OnCircleKick);
        SubscribeLocalEvent<CanPerformComboComponent, Events.SweepKickPerformedEvent>(OnSweepKick);
        SubscribeLocalEvent<CanPerformComboComponent, Events.SpinKickPerformedEvent>(OnSpinKick);

        SubscribeLocalEvent<GrantCapoeiraComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCapoeiraComponent, ExaminedEvent>(OnGrantCQCExamine);
    }

    private void OnSpinKick(Entity<CanPerformComboComponent> ent, ref Events.SpinKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        var velocity = GetVelocity(ent);
        if (!TryPerformCapoeiraMove(ent, args, velocity))
            return;

        var power = GetCapoeiraPower(args, velocity);

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            false,
            proto.DropHeldItemsBehavior);

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(args.Sound, target);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _, TargetBodyPart.Head);
        SpeedUpAttacks(ent, args.AttackSpeedMultiplier, args.AttackSpeedMultiplierTime);

        if (args.Emote != null && TryComp(ent, out AnimatedEmotesComponent? emotes))
        {
            emotes.Emote = args.Emote.Value;
            Dirty(ent, emotes);
        }

        ComboPopup(ent, target, proto.Name);
    }

    private void OnSweepKick(Entity<CanPerformComboComponent> ent, ref Events.SweepKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        var velocity = GetVelocity(ent);
        if (!TryPerformCapoeiraMove(ent, args, velocity))
            return;

        var power = GetCapoeiraPower(args, velocity);

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            false,
            proto.DropHeldItemsBehavior);

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _, TargetBodyPart.Torso);
        _audio.PlayPvs(args.Sound, target);
        SpeedUpAttacks(ent, args.AttackSpeedMultiplier, args.AttackSpeedMultiplierTime);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnCircleKick(Entity<CanPerformComboComponent> ent, ref Events.CircleKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        var velocity = GetVelocity(ent);
        if (!TryPerformCapoeiraMove(ent, args, velocity))
            return;

        var power = GetCapoeiraPower(args, velocity);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage * power, applyResistances: true);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _, TargetBodyPart.Head);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
    }

    private void OnPushKick(Entity<CanPerformComboComponent> ent, ref Events.PushKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        var velocity = GetVelocity(ent);
        if (!TryPerformCapoeiraMove(ent, args, velocity))
            return;

        var power = GetCapoeiraPower(args, velocity);

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            false,
            proto.DropHeldItemsBehavior);

        _audio.PlayPvs(args.Sound, target);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _, TargetBodyPart.Torso);
        _grabThrowing.Throw(target, ent, dir.Normalized() * args.ThrowRange * power, proto.ThrownSpeed);
        ComboPopup(ent, target, proto.Name);
    }

    private void SpeedUpAttacks(EntityUid uid, float multiplier, TimeSpan time)
    {
        var multComp = EnsureComp<Components.MeleeAttackRateMultiplierComponent>(uid);
        multComp.Data.Add(new MeleeAttackRateMultiplierData
        {
            Multiplier = multiplier,
            EndTime = _timing.CurTime + time,
        });

        Dirty(uid, multComp);
    }

    private float GetCapoeiraPower(Events.BaseCapoeiraEvent ev, float velocity)
    {
        return Math.Clamp(velocity * ev.VelocityPowerMultiplier, ev.MinPower, ev.MaxPower);
    }

    private bool TryPerformCapoeiraMove(EntityUid uid, Events.BaseCapoeiraEvent ev, float velocity)
    {
        if (ev.MinVelocity <= velocity)
            return true;

        _popupSystem.PopupEntity(Loc.GetString("capoeira-fail-low-velocity"), uid, uid);
        return false;
    }

    private float GetVelocity(EntityUid uid)
    {
        return TryComp(uid, out PhysicsComponent? physics) ? physics.LinearVelocity.Length() : 0f;
    }
}
