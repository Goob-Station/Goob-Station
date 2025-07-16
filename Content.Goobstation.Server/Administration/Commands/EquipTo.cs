// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Administration;
using Content.Shared.Access.Components;
using Content.Shared.Administration;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

[AdminCommand(AdminFlags.Spawn)]
public sealed class EquipTo : LocalizedCommands
{

    public const string CommandName = "equipto";
    public override string Command => CommandName;

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var invSystem = entityManager.System<InventorySystem>();

        if (args.Length < 3)
        {
            shell.WriteLine(Loc.GetString("cmd-equipto-args-error"));
            return;
        }

        if (!NetEntity.TryParse(args[0], out var targetNet)
            || !entityManager.TryGetEntity(targetNet, out var targetEntity))
        {
            shell.WriteLine(Loc.GetString("cmd-equipto-bad-target", ("target", args[0])));
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
            shell.WriteLine(Loc.GetString("cmd-equipto-bad-proto", ("proto", args[1])));
            return;
        }

        if (!bool.TryParse(args[2], out var deletePrevious))
            return;

        if (args.Length >= 4)
        {
            var targetSlot = args[3];

            invSystem.TryGetSlotEntity(target, targetSlot, out var existing);
            if (invSystem.TryEquip(target, item, targetSlot, force: true, silent: true))
            {
                if (deletePrevious
                    && existing != null)
                    entityManager.DeleteEntity(existing.Value);

                shell.WriteLine(Loc.GetString("cmd-equipto-success",
                    ("item", entityManager.ToPrettyString(item)),
                    ("target", entityManager.ToPrettyString(target)),
                    ("targetSlot", targetSlot)));
            }
            else
            {
                shell.WriteLine(Loc.GetString("cmd-equipto-failure",
                    ("item", entityManager.ToPrettyString(item)),
                    ("target", entityManager.ToPrettyString(target)),
                    ("targetSlot", targetSlot)));
            }
            return;
        }

        var equipped = false;
        if (invSystem.TryGetSlots(target, out var slots)
            && entityManager.TryGetComponent<ClothingComponent>(item, out var clothingComponent))
        {
            foreach (var slot in slots)
            {
                if (!clothingComponent.Slots.HasFlag(slot.SlotFlags))
                    continue;

                if (deletePrevious
                    && invSystem.TryGetSlotEntity(target, slot.Name, out var existing))
                    entityManager.DeleteEntity(existing.Value);
                else
                    invSystem.TryUnequip(target, slot.Name, true, true);

                invSystem.TryEquip(target, item, slot.Name, force: true, silent: true);

                if (slot.Name == "id" &&
                    entityManager.TryGetComponent(item, out PdaComponent? pdaComponent) &&
                    entityManager.TryGetComponent<IdCardComponent>(pdaComponent.ContainedId, out var id))
                {
                    id.FullName = entityManager.GetComponent<MetaDataComponent>(target).EntityName;
                }

                shell.WriteLine(Loc.GetString("cmd-equipto-success",
                    ("item", entityManager.ToPrettyString(item)),
                    ("target", entityManager.ToPrettyString(target)),
                    ("targetSlot", slot.Name)));

                equipped = true;
                break;
            }
        }

        if (equipped)
            return;

        shell.WriteLine(Loc.GetString("cmd-equipto-total-failure",
            ("item", entityManager.ToPrettyString(item)),
            ("target", entityManager.ToPrettyString(target))));

        entityManager.DeleteEntity(item);
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

        if (args.Length != 4
            || !prototypeManager.TryIndex<InventoryTemplatePrototype>("human", out var inventoryTemplate))
            return CompletionResult.Empty;

        var options = inventoryTemplate.Slots.Select(c => c.Name).OrderBy(c => c).ToArray();
        return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-equipto-hint"));
    }
}

