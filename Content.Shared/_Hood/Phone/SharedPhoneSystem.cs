// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Containers.ItemSlots;

namespace Content.Shared._Hood.Phone;

/// <summary>
/// Registers the physical SIM slot on both client and server.
/// </summary>
public abstract class SharedPhoneSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhoneComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<PhoneComponent, ComponentRemove>(OnComponentRemove);
    }

    private void OnComponentInit(Entity<PhoneComponent> ent, ref ComponentInit args)
    {
        _itemSlots.AddItemSlot(ent.Owner, PhoneComponent.SimSlotId, ent.Comp.SimSlot);
    }

    private void OnComponentRemove(Entity<PhoneComponent> ent, ref ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(ent.Owner, ent.Comp.SimSlot);
    }

    public bool TryGetSim(Entity<PhoneComponent> phone, out Entity<SimCardComponent> sim)
    {
        sim = default;

        if (phone.Comp.SimSlot.Item is not { } simUid ||
            !TryComp(simUid, out SimCardComponent? simComp))
        {
            return false;
        }

        sim = (simUid, simComp);
        return true;
    }
}
