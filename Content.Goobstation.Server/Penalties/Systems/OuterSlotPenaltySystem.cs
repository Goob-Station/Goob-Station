using Content.Server.Popups;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Penalties.Systems;

public sealed partial class OuterSlotPenaltySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Shared.Penalties.Components.OuterSlotPenaltyComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<Shared.Penalties.Components.OuterSlotPenaltyComponent, ClothingDidUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, Shared.Penalties.Components.OuterSlotPenaltyComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = true;
            if (_damageableSystem.TryChangeDamage(uid, comp.Damage, true) != null)
                _popupSystem.PopupEntity(Loc.GetString("equipped-outer-slot-with-penalty-message", ("entity", Name(args.Clothing))), uid, uid, PopupType.SmallCaution);
            else
                Logger.Warning($"Damage application failed for entity {uid}. Ensure outer slot penalty is properly configured.");
        }
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnUnequip(EntityUid uid, Shared.Penalties.Components.OuterSlotPenaltyComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = false;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }
}
