using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class EquipTo : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    public string Command => "equipto";

    public string Description => "Equip a given entity to a specified entity.";

    public string Help => $"Usage: {Command} <target> <item> / {Command} <target> <item> <slot>";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var invSystem = _entManager.System<InventorySystem>();
        var clothingSystem = _entManager.System<ClothingSystem>();

        if (args.Length <= 1)
        {
            shell.WriteLine($"Not enough arguments.\n{Help}");
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet) || !_entManager.TryGetEntity(targetNet, out var target))
        {
            shell.WriteLine($"Invalid entity id.");
            return;
        }

        if (!NetEntity.TryParse(args[1], out var itemNet) || !_entManager.TryGetEntity(itemNet, out var item))
        {
            shell.WriteLine($"Invalid item id.");
            return;
        }

        switch (args.Length)
        {
            case 2:
            {
                if (!_entManager.TryGetComponent(item, out ClothingComponent? clothingComp))
                {
                    shell.WriteLine($"The specified item cannot be equipped.");
                    return;
                }

                if (!invSystem.TryEquip(target.Value, item.Value, clothingComp.Slots.ToString().ToLowerInvariant(), true, true))
                    shell.WriteLine($"Failed to equip item.");

                break;
            }
            case 3:
            {
                if (!Enum.TryParse<SlotFlags>(args[2], out var flags))
                {
                    shell.WriteLine($"Invalid slotflags");
                    return;
                }

                var addedClothingComp = _entManager.EnsureComponent<ClothingComponent>(item.Value);
                clothingSystem.SetSlots(item.Value, flags);

                _entManager.DirtyEntity(item.Value);

                if (!invSystem.TryEquip(target.Value, item.Value, addedClothingComp.Slots.ToString().ToLowerInvariant(), true, true))
                    shell.WriteLine($"Failed to equip item.");

                break;
            }
        }
    }
}

