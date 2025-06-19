// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Shared.Abilities.Mime;
using Content.Shared.Administration;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Spawn)]
public sealed class EquipTo : IConsoleCommand
{
    public string Command => "equipto";

    public string Description => "Equip a given entity to a specified entity.";

    public string Help => $"Usage: {Command} <target> <itemUid/ProtoId> <bool-DeletePrevious> / {Command} <target> <itemUid/ProtoId> <bool-DeletePrevious> <slot>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var invSystem = entityManager.System<InventorySystem>();

        if (args.Length < 3)
        {
            shell.WriteLine($"Not enough arguments.\n{Help}");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !entityManager.TryGetEntity(targetNet, out var targetEntity))
        {
            shell.WriteLine($"Invalid target entity: {args[0]}");
            return;
        }
        var target = targetEntity.Value;

        EntityUid item;
        if (NetEntity.TryParse(args[1], out var itemNet) &&
            entityManager.TryGetEntity(itemNet, out var itemEntity))
        {
            item = itemEntity.Value;
        }
        else if (prototypeManager.TryIndex(args[1], out var prototype))
        {
            item = entityManager.SpawnEntity(prototype.ID, entityManager.GetComponent<TransformComponent>(target).Coordinates);
        }
        else
        {
            shell.WriteLine($"Invalid item UID/prototype: {args[1]}");
            return;
        }

        if (!bool.TryParse(args[2], out var deletePrevious))
        {
            shell.WriteLine($"Invalid boolean for deletePrevious: {args[2]}");
            return;
        }

        string? targetSlot = null;

        if (entityManager.TryGetComponent(item, out ClothingComponent? clothingComp))
            targetSlot = clothingComp.Slots.ToString().ToLowerInvariant();

        if (args.Length >= 4)
            targetSlot = args[3];

        if (string.IsNullOrEmpty(targetSlot))
        {
            shell.WriteLine("No valid slot specified and item has no Slot defined.");
            return;
        }

        if (deletePrevious)
        {
            if (invSystem.TryGetSlotEntity(target, targetSlot, out var existing))
                entityManager.DeleteEntity(existing);
        }
        else
        {
            invSystem.DropSlotContents(target, targetSlot);
        }

        if (!invSystem.TryEquip(target, item, targetSlot, force: true, silent: true))
        {
            shell.WriteLine($"Failed to equip {entityManager.ToPrettyString(item)} to {targetSlot}.");
            return;
        }

        shell.WriteLine($"Equipped {entityManager.ToPrettyString(item)} to {entityManager.ToPrettyString(target)} in slot {targetSlot}.");
    }
}

