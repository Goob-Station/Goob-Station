using Content.Shared._Goobstation.Species.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Shared.Inventory;

namespace Content.Server._Goobstation.Species.Systems;

public sealed partial class YowieSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<YowieComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<YowieComponent, ClothingDidUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, YowieComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = true;
            if (_damageableSystem.TryChangeDamage(uid, comp.Damage, true) != null)
            {
                _popupSystem.PopupEntity(Loc.GetString("yowie-eva-suit-equipped-message", ("entity", Name(args.Clothing))), uid, uid, PopupType.SmallCaution);
            }
        }
    }

    private void OnUnequip(EntityUid uid, YowieComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = false;
        }
    }
}
