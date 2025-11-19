// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Emoting;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Goobstation.Shared.Sprinting;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
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
        SubscribeLocalEvent<CanPerformComboComponent, KickUpPerformedEvent>(OnKickUp);

        SubscribeLocalEvent<GrantCapoeiraComponent, UseInHandEvent>(OnGrantCQCUse);
    }

    private void OnCapoeiraMeleeHit(EntityUid uid, ref MeleeHitEvent ev)
    {
        if (ev.HitEntities.Count > 0 || ev.Weapon != uid)
            return;

        // Damage up on miss
        ApplyMultiplier(uid,
            1f,
            2f,
            TimeSpan.FromSeconds(3),
            MartialArtModifierType.Damage | MartialArtModifierType.Unarmed);

        // Miss recovery
        if (!TryComp(uid, out MeleeWeaponComponent? melee))
            return;

        melee.NextAttack -= TimeSpan.FromSeconds(0.75f / _melee.GetAttackRate(uid, uid, melee));
        Dirty(uid, melee);
    }

    private void OnCapoeiraAttackPerformed(Entity<MartialArtsKnowledgeComponent> ent,
        ref ComboAttackPerformedEvent args)
    {
        if (args.Performer == args.Target)
            return;

        if (args.Type == ComboAttackType.Grab)
        {
            ApplyMultiplier(ent, 1.2f, 0f, TimeSpan.FromSeconds(4), MartialArtModifierType.MoveSpeed);
            _modifier.RefreshMovementSpeedModifiers(ent);
            if (!TryComp(args.Target, out SprinterComponent? sprinter))
                return;

            _sprinting.ToggleSprint(args.Target, sprinter, false, false);
            sprinter.LastSprint = _timing.CurTime + TimeSpan.FromSeconds(2); // 5s sprinting delay
            Dirty(args.Target, sprinter);
            return;
        }

        if (args.Weapon != args.Performer
            || args.Type is not (ComboAttackType.Disarm or ComboAttackType.Harm))
            return;

        var velocity = GetVelocity(ent);
        var multiplier = Math.Clamp(MathF.Pow(velocity, 0.2f), 1f, 1.5f);
        ApplyMultiplier(ent, multiplier, 0f, TimeSpan.FromSeconds(3));
    }

    private void OnKickUp(Entity<CanPerformComboComponent> ent, ref KickUpPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _)
            || target != ent.Owner)
            return;

        _status.TryRemoveStatusEffect(ent, "KnockedDown");
        _standingState.Stand(ent);
        //_stamina.TryTakeStamina(ent, args.StaminaToHeal);
        ent.Comp.LastAttacks.Clear();
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

        if (!TryPerformCapoeiraMove(ent, args, out var power))
            return;

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            true,
            proto.DropHeldItemsBehavior);

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _audio.PlayPvs(args.Sound, target);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _);
        ApplyMultiplier(ent, args.AttackSpeedMultiplier, 0f, args.AttackSpeedMultiplierTime);

        if (args.Emote != null && TryComp(ent, out AnimatedEmotesComponent? emotes))
        {
            emotes.Emote = args.Emote.Value;
            Dirty(ent, emotes);
        }

        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnSweepKick(Entity<CanPerformComboComponent> ent, ref SweepKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        if (!TryPerformCapoeiraMove(ent, args, out var power))
            return;

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            true,
            proto.DropHeldItemsBehavior);

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _);
        _audio.PlayPvs(args.Sound, target);
        ApplyMultiplier(ent, args.AttackSpeedMultiplier, 0f, args.AttackSpeedMultiplierTime);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnCircleKick(Entity<CanPerformComboComponent> ent, ref CircleKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        if (!TryPerformCapoeiraMove(ent, args, out var power))
            return;

        var speedMultiplier = 1f / MathF.Max(1f, power);
        _stun.TrySlowdown(target, args.SlowDownTime * power, true, speedMultiplier, speedMultiplier);
        _modifier.RefreshMovementSpeedModifiers(target);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnPushKick(Entity<CanPerformComboComponent> ent, ref PushKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        if (!TryPerformCapoeiraMove(ent, args, out var power))
            return;

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        _stun.TryKnockdown(target,
            TimeSpan.FromSeconds(proto.ParalyzeTime * power),
            true,
            proto.DropHeldItemsBehavior);

        _audio.PlayPvs(args.Sound, target);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage * power, out _);
        _grabThrowing.Throw(target,
            ent,
            dir.Normalized() * args.ThrowRange * power,
            proto.ThrownSpeed,
            behavior: proto.DropHeldItemsBehavior);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void ApplyMultiplier(EntityUid uid,
        float multiplier,
        float modifier,
        TimeSpan time,
        MartialArtModifierType type = MartialArtModifierType.AttackRate)
    {
        if (Math.Abs(multiplier - 1f) < 0.001f
            && Math.Abs(modifier) < 0.001f
            || time <= TimeSpan.Zero)
            return;

        var multComp = EnsureComp<MartialArtModifiersComponent>(uid);
        multComp.Data.Add(new MartialArtModifierData
        {
            Type = type,
            Multiplier = multiplier,
            Modifier = modifier,
            EndTime = _timing.CurTime + time,
        });

        Dirty(uid, multComp);
    }

    private float GetCapoeiraPower(BaseCapoeiraEvent ev, float velocity)
    {
        return Math.Clamp(velocity * ev.VelocityPowerMultiplier, ev.MinPower, ev.MaxPower);
    }

    private bool TryPerformCapoeiraMove(EntityUid uid, BaseCapoeiraEvent ev, out float power)
    {
        var velocity = GetVelocity(uid);

        power = 0;

        if (ev.MinVelocity <= velocity)
        {
            power = GetCapoeiraPower(ev, velocity);
            _stamina.TryTakeStamina(uid, ev.StaminaToHeal);
            return true;
        }

        _popupSystem.PopupEntity(Loc.GetString("capoeira-fail-low-velocity"), uid, uid);
        return false;
    }

    private float GetVelocity(EntityUid uid)
    {
        return TryComp(uid, out PhysicsComponent? physics) ? physics.LinearVelocity.Length() : 0f;
    }
}
