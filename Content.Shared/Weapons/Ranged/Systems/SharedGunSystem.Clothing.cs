using System.Diagnostics.CodeAnalysis;
using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Hands.EntitySystems;

namespace Content.Shared.Weapons.Ranged.Systems;

public partial class SharedGunSystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    private void InitializeClothing()
    {
        SubscribeLocalEvent<ClothingSlotAmmoProviderComponent, TakeAmmoEvent>(OnClothingTakeAmmo);
        SubscribeLocalEvent<ClothingSlotAmmoProviderComponent, GetAmmoCountEvent>(OnClothingAmmoCount);
    }

    private void OnClothingTakeAmmo(EntityUid uid, ClothingSlotAmmoProviderComponent component, TakeAmmoEvent args)
    {
        if (!TryGetClothingSlotEntity(uid, component, out var entity))
            return;
        RaiseLocalEvent(entity.Value, args);
    }

    private void OnClothingAmmoCount(EntityUid uid, ClothingSlotAmmoProviderComponent component, ref GetAmmoCountEvent args)
    {
        if (!TryGetClothingSlotEntity(uid, component, out var entity))
            return;
        RaiseLocalEvent(entity.Value, ref args);
    }

    private bool TryGetClothingSlotEntity(EntityUid uid, ClothingSlotAmmoProviderComponent component, [NotNullWhen(true)] out EntityUid? slotEntity)
    {
        slotEntity = null;

        if (!Containers.TryGetContainingContainer((uid, null, null), out var container))
            return false;
        var user = container.Owner;

        // Assmos extinguisher nozzle changes start here
        if (component.CheckHands)
        {
            foreach (var item in _handsSystem.EnumerateHeld(user))
            {
                if (item == uid)
                    continue;

                if (!_whitelistSystem.IsWhitelistFailOrNull(component.ProviderWhitelist, item))
                {
                    slotEntity = item;
                    return true;
                }
            }
        }
        // Assmos changes end

        if (!_inventory.TryGetContainerSlotEnumerator(user, out var enumerator, component.TargetSlot))
            return false;

        while (enumerator.NextItem(out var item))
        {
            if (_whitelistSystem.IsWhitelistFailOrNull(component.ProviderWhitelist, item))
                continue;

            slotEntity = item;
            return true;
        }

        return false;
    }
}
