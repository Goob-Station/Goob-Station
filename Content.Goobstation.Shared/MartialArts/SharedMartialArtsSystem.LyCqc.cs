using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.Werewolf.Abilities;
using Content.Goobstation.Shared.Werewolf.Abilities.Basic;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Item;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    public void InitializeWerewolf()
    {
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentStartup>(OnWerewolfMovesStartup);
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentShutdown>(OnWerewolfMovesShutdown);

        SubscribeLocalEvent<CanPerformComboComponent, LyCqcOpenVeinPerformedEvent>(OnOpenVeinPerformed);
        SubscribeLocalEvent<CanPerformComboComponent, LyCqcViciousTossEvent>(OnViciousTossPerformed);
        SubscribeLocalEvent<CanPerformComboComponent, LyCqcDismembermentPerformedEvent>(OnDismembermentPerformed);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, PickupAttemptEvent>(OnItemPickUpAttempt);
    }

    #region startup
    private void OnWerewolfMovesStartup(Entity<GrantWerewolfMovesComponent> ent, ref ComponentStartup args)
    {
        if (!_netManager.IsServer)
            return;

        TryGrantMartialArt(ent.Owner, ent.Comp);
    }

    private void OnWerewolfMovesShutdown(Entity<GrantWerewolfMovesComponent> ent, ref ComponentShutdown args)
    {
        var user = ent.Owner;
        if (!HasComp<MartialArtsKnowledgeComponent>(user))
            return;

        RemComp<MartialArtsKnowledgeComponent>(user);
        RemComp<CanPerformComboComponent>(user);
    }
    #endregion

    private void OnOpenVeinPerformed(Entity<CanPerformComboComponent> ent, ref LyCqcOpenVeinPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed))
            return;

        DoDamage(ent.Owner, target, proto.DamageType, proto.ExtraDamage, out _, TargetBodyPart.Legs);
        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);
        ComboPopup(ent.Owner, target, proto.Name);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnViciousTossPerformed(Entity<CanPerformComboComponent> ent, ref LyCqcViciousTossEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed
            || !TryComp<PullerComponent>(ent, out _)
            || !TryComp<PullableComponent>(target, out var pullable))
            return;

        _pulling.TryStopPull(target, pullable, ent.Owner, true);
        _grabThrowing.Throw(
            target,
            ent.Owner,
            _transform.GetWorldRotation(ent).ToWorldVec(),
            args.ThrowSpeed,
            args.DamageThrow);

        _stamina.TakeStaminaDamage(target, proto.StaminaDamage, applyResistances: true);
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit3.ogg"), target);
        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnDismembermentPerformed(Entity<CanPerformComboComponent> ent, ref LyCqcDismembermentPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || !TryComp<BodyComponent>(target, out var body))
            return;

        RipLimb(target, body);
        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void RipLimb(EntityUid target, BodyComponent body)
    {
        var hands = _body.GetBodyChildrenOfType(target, BodyPartType.Arm, body).ToList();

        if (hands.Count <= 0)
            return;

        var pick = _random.Pick(hands);

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_transform.ogg"), target);
        _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);
    }

    private void OnItemPickUpAttempt(Entity<MartialArtsKnowledgeComponent> ent, ref PickupAttemptEvent args)
    {
        if (ent.Comp.MartialArtsForm != MartialArtsForms.LyCqc)
            return;
        _popupSystem.PopupClient(Loc.GetString("rogue-ascended-shatter-fail"), ent, ent); // goida
        args.Cancel();
    }
}
