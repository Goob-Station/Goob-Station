using Content.Shared.Species.Components;
using Content.Shared.Clothing;
using Content.Shared.Movement.Systems;
using Content.Shared.Clothing.Components;
using Robust.Shared.Containers;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;

namespace Content.Server.Species.Systems;

public sealed partial class YowieSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<YowieComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<YowieComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<YowieComponent, ClothingDidEquippedEvent>(OnEquip);
        SubscribeLocalEvent<YowieComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
    }

    private void OnEntInserted(EntityUid uid, YowieComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth))
        {
            if (cloth.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
            {
                cloth.EquipDelay = TimeSpan.FromSeconds(comp.EquipDelay);
                cloth.UnequipDelay = TimeSpan.FromSeconds(comp.UnequipDelay);
            }
        }
    }

    private void OnEntRemoved(EntityUid uid, YowieComponent comp, ref EntRemovedFromContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth))
        {
            if (cloth.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
            {
                cloth.EquipDelay = default!;
                cloth.UnequipDelay = default!;
            }
        }
    }

    private void OnEquip(EntityUid uid, YowieComponent comp, ref ClothingDidEquippedEvent args)
    {
        if (args.Clothing.Comp.Slots == Shared.Inventory.SlotFlags.OUTERCLOTHING)
        {
            comp.OuterLayerEquipped = true;
            _damageableSystem.TryChangeDamage(
            uid,
            comp.Damage,
            true
            );
        }
    }

    private void OnMove(EntityUid uid, YowieComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.OuterLayerEquipped)
        {
            args.ModifySpeed(comp.SoftSuitSpeedMultiplier);
        }
    }
}
