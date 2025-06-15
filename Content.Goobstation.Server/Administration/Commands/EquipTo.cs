// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Console;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class EquipTo : IConsoleCommand
{
    public string Command => "equipto";

    public string Description => "Equip a given entity to a specified entity.";

    public string Help => $"Usage: {Command} <target> <item> <bool-DeletePrevious> / {Command} <target> <item> <bool-DeletePrevious> <slot>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();

        var invSystem = entityManager.System<InventorySystem>();
        var clothingSystem = entityManager.System<ClothingSystem>();

        string targetSlot = null!;

        if (args.Length < 3)
        {
            shell.WriteLine($"Not enough arguments.\n{Help}");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !entityManager.TryGetEntity(targetNet, out var targetNullable)
            || targetNullable is not { } target)
        {
            shell.WriteLine($"Invalid EntityUid of {args[0]}");
            return;
        }

        if (!NetEntity.TryParse(args[1], out var itemNet)
            || !entityManager.TryGetEntity(itemNet, out var itemNullable)
            || itemNullable is not { } item)
        {
            shell.WriteLine($"Invalid ItemUid of {args[1]}");
            return;
        }

        if (entityManager.TryGetComponent(item, out ClothingComponent? clothingComp))
            targetSlot = clothingComp.Slots.ToString().ToLowerInvariant();

        if (bool.TryParse(args[2], out var deletePrevious))
        {
            if (deletePrevious)
            {
                invSystem.TryGetSlotEntity(target, targetSlot, out var slotEntity);
                entityManager.DeleteEntity(slotEntity);

            }
        }

        if (Enum.TryParse<SlotFlags>(args[3], out var flags))
        {
            entityManager.EnsureComponent<ClothingComponent>(item);
            clothingSystem.SetSlots(item, flags);
            entityManager.DirtyEntity(item);

            targetSlot = flags.ToString().ToLowerInvariant();
        }

        invSystem.DropSlotContents(target, targetSlot);

        if (!invSystem.TryEquip(target, item, targetSlot, true, true))
        {
            shell.WriteLine($"Could not equip {entityManager.ToPrettyString(item)}");
            return;
        }

        shell.WriteLine(
            $"Equipped {entityManager.ToPrettyString(item)} to {entityManager.ToPrettyString(target)}");
    }
}

