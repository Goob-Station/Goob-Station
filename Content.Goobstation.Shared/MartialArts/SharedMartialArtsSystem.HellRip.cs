// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 Bokser815 <70928915+Bokser815@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lincoln McQueen <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Events;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;

// Shitmed Change
using Content.Shared.Body.Part;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Components;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    [Dependency] private readonly WoundSystem _wound = default!; // Shitmed Change

    private void InitializeHellRip()
    {
        SubscribeLocalEvent<CanPerformComboComponent, HellRipSlamPerformedEvent>(OnHellRipSlam);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipDropKickPerformedEvent>(OnHellRipDropKick);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipHeadRipPerformedEvent>(OnHellRipHeadRip);
        SubscribeLocalEvent<CanPerformComboComponent, HellRipTearDownPerformedEvent>(OnHellRipTearDown);

        SubscribeLocalEvent<GrantHellRipComponent, MapInitEvent>(OnGrantHellRip);
        SubscribeLocalEvent<GrantHellRipComponent, ComponentShutdown>(OnRemoveHellRip);
        SubscribeLocalEvent<GrantHellRipComponent, UseInHandEvent>(OnGrantCQCUse);
    }

    #region Generic Methods

    private void OnGrantHellRip(Entity<GrantHellRipComponent> ent, ref MapInitEvent args)
    {
        if (!HasComp<MobStateComponent>(ent))
            return;

        TryGrantMartialArt(ent.Owner, ent.Comp);
    }


    private void OnRemoveHellRip(Entity<GrantHellRipComponent> ent, ref ComponentShutdown args)
    {
        var user = ent.Owner;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }

    #endregion

    #region Combo Methods

    private void OnHellRipHeadRip(Entity<CanPerformComboComponent> ent, ref HellRipHeadRipPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || !_mobState.IsDead(target)
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);

        _pulling.TryStopPull(target, pullable, ent, true);

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Blunt", 300);
        _damageable.TryChangeDamage(target, damage, true, origin: ent, targetPart: TargetBodyPart.Head);
        var head = _body.GetBodyChildrenOfType(target , BodyPartType.Head).FirstOrDefault();
        if (head != default
            && TryComp<WoundableComponent>(head.Id, out var woundable)
            && woundable.ParentWoundable.HasValue)
            _wound.AmputateWoundable(woundable.ParentWoundable.Value, head.Id, woundable);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/gib1.ogg"), target);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/demon_attack1.ogg"), ent);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnHellRipSlam(Entity<CanPerformComboComponent> ent, ref HellRipSlamPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        var knockdownTime = TimeSpan.FromSeconds(proto.ParalyzeTime);

        var ev = new BeforeStaminaDamageEvent(1f);
        RaiseLocalEvent(target, ref ev);

        knockdownTime *= ev.Value;

        _stun.TryKnockdown(target, knockdownTime, true, proto.DropHeldItemsBehavior);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        //_pulling.TryStopPull(target, pullable, ent, true);

        _status.TryRemoveStatusEffect(ent, "KnockedDown");
        _standingState.Stand(ent);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/demon_attack1.ogg"), ent);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/metal_slam5.ogg"), target);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnHellRipDropKick(Entity<CanPerformComboComponent> ent, ref HellRipDropKickPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || !downed
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);

        _pulling.TryStopPull(target, pullable, ent, true);
        //_grabThrowing.Throw(target, ent, _transform.GetMapCoordinates(ent).Position + _transform.GetMapCoordinates(target).Position, 50);
        var entPos = _transform.GetMapCoordinates(ent).Position;
        var targetPos = _transform.GetMapCoordinates(target).Position;
        var direction = targetPos - entPos; // vector from ent to target

        _grabThrowing.Throw(target, ent, direction, 25);

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/demon_attack1.ogg"), ent);
        ComboPopup(ent, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnHellRipTearDown(Entity<CanPerformComboComponent> ent,
       ref HellRipTearDownPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !_proto.TryIndex<MartialArtPrototype>(proto.MartialArtsForm.ToString(), out var martialArtProto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _pulling.TryStopPull(target, pullable, ent, true);

        DoDamage(ent, target, proto.DamageType, proto.ExtraDamage, out _);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Fluids/blood1.ogg"), target);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/demon_attack1.ogg"), ent);

    }
    #endregion
}
