using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
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
        SubscribeLocalEvent<CanPerformComboComponent, PushKickPerformedEvent>(OnPushKick);
        SubscribeLocalEvent<CanPerformComboComponent, CircleKickPerformedEvent>(OnCircleKick);
        SubscribeLocalEvent<CanPerformComboComponent, SweepKickPerformedEvent>(OnSweepKick);
        SubscribeLocalEvent<CanPerformComboComponent, SpinKickPerformedEvent>(OnSpinKick);

        SubscribeLocalEvent<GrantCapoeiraComponent, UseInHandEvent>(OnGrantCQCUse);
        SubscribeLocalEvent<GrantCapoeiraComponent, ExaminedEvent>(OnGrantCQCExamine);
    }

    private void OnCapoeiraAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent, ref ComboAttackPerformedEvent args)
    {
        if (args.Type == ComboAttackType.Grab)
        {
            ApplyMultiplier(ent, 1.2f, TimeSpan.FromSeconds(4), MartialArtModifierType.MoveSpeed);
            _modifier.RefreshMovementSpeedModifiers(ent);
            return;
        }

        if (args.Weapon != args.Performer || args.Type is not (ComboAttackType.Disarm or ComboAttackType.Harm))
            return;

        var velocity = GetVelocity(ent);
        var multiplier = Math.Clamp(MathF.Pow(velocity, 0.2f), 1f, 1.5f);
        ApplyMultiplier(ent, multiplier, TimeSpan.FromSeconds(3));
    }

    private void OnSpinKick(Entity<CanPerformComboComponent> ent, ref SpinKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (downed)
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-target-down"), ent, ent);
            return;
        }

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
        ApplyMultiplier(ent, args.AttackSpeedMultiplier, args.AttackSpeedMultiplierTime);

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
        ApplyMultiplier(ent, args.AttackSpeedMultiplier, args.AttackSpeedMultiplierTime);
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

    private void ApplyMultiplier(EntityUid uid,
        float multiplier,
        TimeSpan time,
        MartialArtModifierType type = MartialArtModifierType.AttackRate)
    {
        if (Math.Abs(multiplier - 1f) < 0.001f || time <= TimeSpan.Zero)
            return;

        var multComp = EnsureComp<MartialArtModifiersComponent>(uid);
        multComp.Data.Add(new MartialArtModifierData
        {
            Type = type,
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
