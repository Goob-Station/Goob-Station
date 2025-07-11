// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Shared.MartialArts;

public abstract partial class SharedMartialArtsSystem
{
    private void InitializeDragon()
    {
        SubscribeLocalEvent<CanPerformComboComponent, DragonClawPerformedEvent>(OnDragonClaw);
        SubscribeLocalEvent<CanPerformComboComponent, DragonTailPerformedEvent>(OnDragonTail);
        SubscribeLocalEvent<CanPerformComboComponent, DragonStrikePerformedEvent>(OnDragonStrike);

        SubscribeLocalEvent<GrantKungFuDragonComponent, UseInHandEvent>(OnGrantCQCUse);

        SubscribeLocalEvent<DragonPowerBuffComponent, AttackedEvent>(OnAttacked);
    }

    private void OnAttacked(Entity<DragonPowerBuffComponent> ent, ref AttackedEvent args)
    {
        if (_hands.TryGetActiveItem(ent.Owner, out _) // Only unarmed
            || !_blocker.CanInteract(ent, null)) // Should be able to interact
            return;

        args.ModifiersList.Add(ent.Comp.ModifierSet);

        // Works for both armed and unarmed attacks
        ApplyMultiplier(ent,
            ent.Comp.DamageMultiplier,
            0f,
            ent.Comp.AttackDamageBuffDuration,
            MartialArtModifierType.Damage);
    }

    private void OnDragonStrike(Entity<CanPerformComboComponent> ent, ref DragonStrikePerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (!downed)
        {
            _popupSystem.PopupEntity(Loc.GetString("martial-arts-fail-target-standing"), ent, ent);
            return;
        }

        // Paralyze, not knockdown
        _stun.TryParalyze(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnDragonTail(Entity<CanPerformComboComponent> ent, ref DragonTailPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, ent, true);

        if (downed)
            _stun.TryStun(target, args.DownedParalyzeTime, true); // No stunlocks
        else
        {
            _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(proto.ParalyzeTime), true, proto.DropHeldItemsBehavior);
            DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        }

        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }


    private void OnDragonClaw(Entity<CanPerformComboComponent> ent, ref DragonClawPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out _))
            return;

        _stun.TrySlowdown(target, args.SlowdownTime, true, args.WalkSpeedModifier, args.SprintSpeedModifier);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);
        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _audio.PlayPvs(args.Sound, target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }
}
