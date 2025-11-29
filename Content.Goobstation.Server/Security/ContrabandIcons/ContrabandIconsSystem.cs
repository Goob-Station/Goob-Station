using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared._Goobstation.Security.ContrabandIcons;
using Content.Shared.Access.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.PDA;

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
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequippHands);
    }

    private void OnMapInit(EntityUid uid, VisibleContrabandComponent component, MapInitEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnEquip(EntityUid uid, VisibleContrabandComponent component, DidEquipEvent args)
    {
        ContrabandDetect(args.Equipee, component, args.SlotFlags);
    }

    private void OnUnequip(EntityUid uid, VisibleContrabandComponent component, DidUnequipEvent args)
    {
        ContrabandDetect(args.Equipee, component, args.SlotFlags);
    }

    private void OnUnequippHands(EntityUid uid, VisibleContrabandComponent component, DidUnequipHandEvent args)
    {
        if(HasComp<IdCardComponent>(args.Unequipped) && uid == args.User)
            return;
        ContrabandDetect(args.User, component);
    }

    private void OnEquipHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        ContrabandDetect(args.User, component);
    }
}    