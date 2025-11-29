using Content.Goobstation.Shared.Security.ContrabandIcons.Components;
using Content.Shared._Goobstation.Security.ContrabandIcons;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;

namespace Content.Goobstation.Server.Security.ContrabandIcons;

public sealed class ContrabandIconsSystem : SharedContrabandIconsSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VisibleContrabandComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipEvent>(OnEquipped);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipEvent>(OnUnequipped);

        SubscribeLocalEvent<VisibleContrabandComponent, DidEquipHandEvent>(OnEquippedHands);
        SubscribeLocalEvent<VisibleContrabandComponent, DidUnequipHandEvent>(OnUnequippedHands);
    }


    private void OnMapInit(EntityUid uid, VisibleContrabandComponent component, MapInitEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnEquipped(EntityUid uid, VisibleContrabandComponent component, DidEquipEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequipped(EntityUid uid, VisibleContrabandComponent component, DidUnequipEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnUnequippedHands(EntityUid uid, VisibleContrabandComponent component, DidUnequipHandEvent args)
    {
        ContrabandDetect(uid, component);
    }

    private void OnEquippedHands(EntityUid uid, VisibleContrabandComponent component, DidEquipHandEvent args)
    {
        ContrabandDetect(uid, component);
    }
}