using Content.Shared.Actions;
using Content.Shared.Changeling;
using Content.Shared.Changeling.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Changeling.EntitySystems;

public sealed partial class ChangelingSystem : EntitySystem
{
    private void SubscribeAbilitiesBase()
    {
        SubscribeLocalEvent<ChangelingComponent, ChangelingInstantActionEvent>(OnInstantAction);
        SubscribeLocalEvent<ChangelingComponent, ChangelingTargetActionEvent>(OnTargetAction);
    }

    #region Helper Methods

    public void PlayMeatySound(EntityUid uid, ChangelingComponent comp)
    {
        var rand = _rand.Next(0, comp.AbilitySoundPool.Count - 1);
        var sound = comp.AbilitySoundPool.ToArray()[rand];
        _audio.PlayPvs(sound, uid, AudioParams.Default.WithVolume(-3f));
    }

    public bool TryUseAbility(EntityUid uid, ChangelingComponent comp, BaseActionEvent action)
    {
        if (action.Handled)
            return false;

        if (!TryComp<ChangelingActionComponent>(action.Action, out var lingAction))
            return false;

        if (!lingAction.UseInLesserForm && comp.IsInLesserForm)
        {
            _popup.PopupEntity(Loc.GetString("changeling-action-fail-lesserform"), uid, uid);
            return false;
        }

        var price = lingAction.ChemicalCost;
        if (comp.Chemicals < price)
        {
            _popup.PopupEntity(Loc.GetString("changeling-chemicals-deficit"), uid, uid);
            return false;
        }

        if (lingAction.RequireAbsorbed > comp.TotalAbsorbedEntities)
        {
            var delta = lingAction.RequireAbsorbed - comp.TotalAbsorbedEntities;
            _popup.PopupEntity(Loc.GetString("changeling-action-fail-absorbed", ("number", delta)), uid, uid);
            return false;
        }

        UpdateChemicals(uid, comp, -price);

        action.Handled = true;

        if (lingAction.Audible)
            PlayMeatySound(uid, comp);

        return true;
    }
    public bool TrySting(EntityUid source, EntityTargetActionEvent? ev = null, bool force = false)
    {
        if (force)
            return true;

        else if (ev == null)
            return false;

        var target = ev.Target;

        if (HasComp<ChangelingComponent>(target))
        {
            var selfMessage = Loc.GetString("changeling-sting-fail-self", ("target", Identity.Entity(target, EntityManager)));
            var targetMessage = Loc.GetString("changeling-sting-fail-ling");

            _popup.PopupEntity(selfMessage, source, source);
            _popup.PopupEntity(targetMessage, target, target);
            return false;
        }
        return true;
    }
    public bool TryReagentSting(EntityUid target, Dictionary<EntProtoId, FixedPoint2> reagents)
    {
        if (reagents.Count == 0)
            return false;

        var solution = new Solution();
        foreach (var reagent in reagents)
            solution.AddReagent(reagent.Key, reagent.Value);

        if (!_solution.TryGetInjectableSolution(target, out var targetSolution, out var _))
            return false;

        if (!_solution.TryAddSolution(targetSolution.Value, solution))
            return false;

        return true;
    }

    public bool TryToggleItem(EntityUid uid, ChangelingComponent comp, ChangelingEquipmentData data)
    {
        return TryToggleItem(uid, comp, data.Prototype, data.EquipmentSlot);
    }
    public bool TryToggleItem(EntityUid uid, ChangelingComponent comp, EntProtoId? proto, string? clothingSlot = null)
    {
        if (proto == null)
            return false;

        // check if the item does not exist
        if (!comp.Equipment.TryGetValue((EntProtoId) proto, out var item) && (item == null || item.Entity == null))
        {
            item = new ChangelingEquipmentData(proto, Spawn(proto, Transform(uid).Coordinates), clothingSlot);

            if (item.Entity == null)
                return false;

            // check if we have an predefined slot and put the item there
            if (clothingSlot != null && !_inventory.TryEquip(uid, (EntityUid) item.Entity, clothingSlot, force: true))
            {
                _popup.PopupEntity(Loc.GetString("changeling-equip-outer-fail"), uid, uid);
                QueueDel(item.Entity);
                return false;
            }
            // if not, we're going to spawn it in a hand
            else if (!_hands.TryForcePickupAnyHand(uid, (EntityUid) item.Entity))
            {
                _popup.PopupEntity(Loc.GetString("changeling-fail-hands"), uid, uid);
                QueueDel(item.Entity);
                return false;
            }

            if (_inventory.TryGetContainingSlot((EntityUid) item.Entity, out var slotDef) && item.EquipmentSlot == null)
                item.EquipmentSlot = slotDef.SlotGroup;

            comp.Equipment.Add((EntProtoId) proto, item);

            return true;
        }
        // so it did exist! get rid of it.
        else if (item != null && item.Entity != null)
        {
            QueueDel(item.Entity);
            comp.Equipment.Remove((EntProtoId) proto);
        }

        return true;
    }

    #endregion

    #region Base Processing

    private void HandleBehaviorCustom(EntityUid ent, ChangelingActionBehaviorCustomComponent comp)
    {
        RaiseLocalEvent(comp.Event);
    }
    private void HandleBehaviorEquip(Entity<ChangelingComponent> ent, ChangelingActionBehaviorEquipComponent comp)
    {
        if (comp.Equipment.Count > 0)
        {
            foreach (var item in comp.Equipment)
                if (item != null)
                    TryToggleItem(ent, ent.Comp, item);
        }
    }

    private void OnInstantAction(Entity<ChangelingComponent> ent, ref ChangelingInstantActionEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (TryComp<ChangelingActionBehaviorEquipComponent>(args.Action, out var behaviorEquip))
            HandleBehaviorEquip(ent, behaviorEquip);

        if (TryComp<ChangelingActionBehaviorCustomComponent>(args.Action, out var behaviorCustom))
            HandleBehaviorCustom(args.Action, behaviorCustom);
    }
    private void OnTargetAction(Entity<ChangelingComponent> ent, ref ChangelingTargetActionEvent args)
    {
        if (!TryUseAbility(ent, ent.Comp, args))
            return;

        if (TryComp<ChangelingActionBehaviorStingComponent>(args.Action, out var behaviorSting))
        {
            if (!TrySting(ent, args, behaviorSting.TargetSelf))
                return;
            TryReagentSting(args.Target, behaviorSting.Reagents);
        }

        if (TryComp<ChangelingActionBehaviorCustomComponent>(args.Action, out var behaviorCustom))
            RaiseLocalEvent(behaviorCustom.Event);
    }

    #endregion
}
