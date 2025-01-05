using Content.Shared._Goobstation.Species.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Popups;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Client._Goobstation.Species.Systems;

public sealed partial class YowieSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _clothingSpeedModifierSystem = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<YowieComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<YowieComponent, ClothingDidUnequippedEvent>(OnUnequip);
    }

    private void OnEquip(EntityUid uid, YowieComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = true;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnUnequip(EntityUid uid, YowieComponent comp, ref ClothingDidUnequippedEvent args)
    {
        if (args.Clothing.Comp.Slots == SlotFlags.OUTERCLOTHING)
            comp.OuterLayerEquipped = false;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }
}
