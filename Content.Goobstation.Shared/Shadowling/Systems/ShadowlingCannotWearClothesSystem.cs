using Content.Shared._EE.Clothing.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Popups;
using Content.Shared.Radio.Components;
using Content.Shared.Storage;


namespace Content.Shared._EE.Clothing.Systems;

public sealed class ShadowlingCannotWearClothesSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingCannotWearClothesComponent, IsEquippingAttemptEvent>(OnTryEquip);
    }

    private void OnTryEquip(EntityUid uid, ShadowlingCannotWearClothesComponent comp, IsEquippingAttemptEvent args)
    {
        // They can equip stuff on other targets
        if (args.EquipTarget != args.Equipee)
            return;

        if (HasComp<ClothingComponent>(args.Equipment)
            && !HasComp<StorageComponent>(args.Equipment)
            && !HasComp<HeadsetComponent>(args.Equipment))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-on-try-equip-clothes"), args.Equipee, PopupType.SmallCaution);
            args.Cancel();
        }

    }
}
