// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
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
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Administration.Commands;

    [AdminCommand(AdminFlags.Spawn)]
    public sealed class SpawnEquippedCommand : IConsoleCommand
    {
        public string Command => "spawnequipped";
        public string Description => "Spawns an entity and immediately equips it to another entity.";

        public string Help =>
            $"Usage: {Command} <entityProtoId> <target> <bool-DeletePrevious> / {Command} <entityProtoId> <target> <bool-DeletePrevious> <slot>";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            var entityManager = IoCManager.Resolve<IEntityManager>();

            var invSystem = entityManager.System<InventorySystem>();
            var clothingSystem = entityManager.System<ClothingSystem>();

            string targetSlot = null!;

            if (args.Length < 3)
            {
                shell.WriteLine($"Not enough arguments.\n{Help}");
                return;
            }

            if (!prototypeManager.TryIndex(args[0], out var prototype))
            {
                shell.WriteError($"No {nameof(EntProtoId)} found with id {args[0]}");
                return;
            }

            if (!NetEntity.TryParse(args[1], out var targetNet)
                || !entityManager.TryGetEntity(targetNet, out var targetNullable)
                || targetNullable is not { } target)
            {
                shell.WriteLine($"Invalid EntityUid of {args[1]}");
                return;
            }

            var spawnedItem = entityManager.SpawnEntity(prototype.ID,
                entityManager.GetComponent<TransformComponent>(target).Coordinates);

            if (entityManager.TryGetComponent(spawnedItem, out ClothingComponent? clothingComp))
                targetSlot = clothingComp.Slots.ToString().ToLowerInvariant();

            if (bool.TryParse(args[2], out var deletePrevious))
            {
                if (deletePrevious)
                    if (invSystem.TryGetSlotEntity(target, targetSlot, out var slotEntity))
                        entityManager.DeleteEntity(slotEntity);
            }

            if (Enum.TryParse<SlotFlags>(args[3], out var flags))
            {
                entityManager.EnsureComponent<ClothingComponent>(spawnedItem);
                clothingSystem.SetSlots(spawnedItem, flags);
                entityManager.DirtyEntity(spawnedItem);

                targetSlot = flags.ToString().ToLowerInvariant();
            }

            invSystem.DropSlotContents(target, targetSlot);

            if (!invSystem.TryEquip(target, spawnedItem, targetSlot, true, true))
            {
                shell.WriteLine($"Could not equip {entityManager.ToPrettyString(spawnedItem)}");
                return;
            }

            shell.WriteLine(
                $"Spawned {entityManager.ToPrettyString(spawnedItem)} and equipped to {entityManager.ToPrettyString(target)}");
        }
    }
