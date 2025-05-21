using Content.Shared.Access.Systems;
using Content.Shared.Contraband;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.CriminalRecords;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.Security.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared.Security.Systems;

public sealed class CriminalStatusSystem : EntitySystem
{
    [Dependency] private readonly SharedIdCardSystem _id = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CriminalRecordComponent, CriminalRecordChanged>(OnCriminalRecordChanged);

        SubscribeLocalEvent<CriminalRecordComponent, ClothingDidEquippedEvent>((u, c, a) => OnEquippedOrUniquip(u, c, true, a.Clothing.Owner, a.Clothing.Comp));
        SubscribeLocalEvent<CriminalRecordComponent, ClothingDidUnequippedEvent>((u, c, a) => OnEquippedOrUniquip(u, c, false, a.Clothing.Owner, a.Clothing.Comp));

        SubscribeLocalEvent<CriminalRecordComponent, DidEquipHandEvent>((u, c, a) => OnPickupOrDrop(u, c, a.Equipped, true));
        SubscribeLocalEvent<CriminalRecordComponent, DidUnequipHandEvent>((u, c, a) => OnPickupOrDrop(u, c, a.Unequipped, false));
    }

    private void OnCriminalRecordChanged(EntityUid uid, CriminalRecordComponent component, CriminalRecordChanged args)
    {
        component.Points -= component.SecurityStatusPoints[args.PreviousStatus];
        component.Points += component.SecurityStatusPoints[args.Status];
    }

    private bool OnEquippedOrUniquip(EntityUid uid, CriminalRecordComponent component, bool equip, EntityUid clothingUid, ClothingComponent? clothingComp = null, bool checkId = true)
    {
        if (!Resolve(clothingUid, ref clothingComp, false))
            return false;

        if (clothingComp.InSlot == null)
            return false;

        if (checkId && UpdateIdCard((uid, component), clothingUid))
            return true;

        if (!TryComp<ContrabandComponent>(clothingUid, out var contraband))
            return true;

        if (contraband.CriminalPoints == 0f)
            return true;

        if (!_inventory.TryGetSlots(uid, out var slots))
            return true;

        SlotFlags? slot = null;

        foreach (var invSlot in slots)
        {
            if (clothingComp.InSlot != invSlot.Name)
                continue;

            slot = invSlot.SlotFlags;
            break;
        }

        if (slot == null)
            return true;

        if (!component.ClothingSlotPoints.TryGetValue(slot.Value, out var slotMultiplier))
            return true;

        if (CheckIdCard(uid, contraband))
            return true;

        var points = GetPoints(clothingUid, contraband.CriminalPoints);

        if (equip)
            component.Points += points;
        else
            component.Points -= points;

        return true;
    }

    private bool OnPickupOrDrop(EntityUid uid, CriminalRecordComponent component, EntityUid item, bool pickup, bool checkId = true)
    {
        if (checkId && UpdateIdCard((uid, component), item))
            return true;

        if (!TryComp<ContrabandComponent>(item, out var contraband))
            return false;

        if (contraband.CriminalPoints == 0f)
            return true;

        if (CheckIdCard(uid, contraband))
            return true;

        var points = GetPoints(item, contraband.CriminalPoints);

        if (pickup)
            component.Points += points;
        else
            component.Points -= points;

        return true;
    }

    private bool CheckIdCard(EntityUid uid, ContrabandComponent contraband)
    {
        List<ProtoId<DepartmentPrototype>>? departments = null;
        if (_id.TryFindIdCard(uid, out var id))
        {
            departments = id.Comp.JobDepartments;
        }

        return contraband.AllowedDepartments != null && departments != null && departments.Intersect(contraband.AllowedDepartments).Any();
    }

    private bool UpdateIdCard(Entity<CriminalRecordComponent> ent, EntityUid item)
    {
        // if access has changed we need to recalculate
        if (!_id.TryGetIdCard(item, out _))
            return false;

        RecalculatePoints(ent);
        return true;
    }

    private float GetPoints(EntityUid uid, float points)
    {
        var ev = new GetCriminalPointsEvent(points);
        RaiseLocalEvent(uid, ev);

        return ev.Points;
    }

    private void RecalculatePoints(Entity<CriminalRecordComponent> ent)
    {
        ent.Comp.Points = 0f;

        foreach (var item in _inventory.GetHandOrInventoryEntities(ent.Owner))
        {
            if (OnEquippedOrUniquip(ent.Owner, ent.Comp, true, item, checkId: false))
                continue;

            OnPickupOrDrop(ent.Owner, ent.Comp, item, true, checkId: false);
        }
    }
}
