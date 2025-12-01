using Content.Goobstation.Shared.Security.ContrabandIcons;
using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared.Access.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Strip.Components;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisibleContrabandComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquip);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequip);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquipHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequipHands);
    }
    
    private void OnMapInit(EntityUid uid, VisibleContrabandComponent component, MapInitEvent args)
    {
        ContrabandDetect(uid, component, SlotFlags.WITHOUT_POCKET);
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent component, DidEquipEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent component, DidUnequipEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequipHands(EntityUid uid, VisibleContrabandComponent component, DidUnequipHandEvent args)
    {
        if (!HasComp<IdCardComponent>(args.Unequipped) && uid == args.User)
            ContrabandDetect(uid, component);
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        if(!HasComp<ThievingComponent>(args.User))
            ContrabandDetect(uid, component);
    }
}
