using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Movement.Pulling.Components;
using Robust.Shared.Random;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    /// <inheritdoc/>
    public void InitializeWerewolf()
    {
        // Component lifetime related
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentStartup>(OnWerewolfMovesStartup);
        SubscribeLocalEvent<GrantWerewolfMovesComponent, ComponentShutdown>(OnWerewolfMovesShutdown);

        // Combos
        SubscribeLocalEvent<CanPerformComboComponent, OpenVeinPerformedEvent>(OnOpenVeinPerformed);
        SubscribeLocalEvent<CanPerformComboComponent, ViciousTossPerformedEvent>(OnViciousTossPerformed);
        SubscribeLocalEvent<CanPerformComboComponent, DismembermentPerformedEvent>(OnDismembermentPerformed);
    }

    #region Generic Events
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

    #region Martial Arts

    private void OnOpenVeinPerformed(Entity<CanPerformComboComponent> ent, ref OpenVeinPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed)
            return;

        DoDamage(ent.Owner, target, proto.DamageType, proto.ExtraDamage, out _);

        if (_netManager.IsServer)
        {
            TryModifyBleeding(target, args.BleedAmount);
        }

        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnViciousTossPerformed(Entity<CanPerformComboComponent> ent, ref ViciousTossPerformedEvent args)
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

        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    private void OnDismembermentPerformed(Entity<CanPerformComboComponent> ent, ref DismembermentPerformedEvent args)
    {
        if (!_proto.TryIndex(ent.Comp.BeingPerformed, out var proto)
            || !TryUseMartialArt(ent, proto, out var target, out var downed)
            || downed
            || !TryComp<BodyComponent>(target, out var body))
            return;

        RipLimb(target, body);

        ComboPopup(ent.Owner, target, proto.Name);
        ent.Comp.LastAttacks.Clear();
    }

    #endregion

    #region Generic Methods

    private void RipLimb(EntityUid target, BodyComponent body)
    {
        var hands = _body.GetBodyChildrenOfType(target, BodyPartType.Hand, body).ToList();

        if (hands.Count <= 0)
            return;

        var pick = _random.Pick(hands);

        if (!TryComp<WoundableComponent>(pick.Id, out var woundable)
            || !woundable.ParentWoundable.HasValue)
            return;

        _wounds.AmputateWoundableSafely(woundable.ParentWoundable.Value, pick.Id, woundable);
    }
    #endregion

    #region Server-Side

    protected virtual void TryModifyBleeding(EntityUid target, float amount) {}

    #endregion
}
