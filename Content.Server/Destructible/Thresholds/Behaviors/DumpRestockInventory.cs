// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Random;
using Content.Shared.Stacks;
using Content.Shared.Prototypes;
using Content.Shared.VendingMachines;

namespace Content.Server.Destructible.Thresholds.Behaviors
{
    /// <summary>
    ///     Spawns a portion of the total items from one of the canRestock
    ///     inventory entries on a VendingMachineRestock component.
    /// </summary>
    [Serializable]
    [DataDefinition]
    public sealed partial class DumpRestockInventory: IThresholdBehavior
    {
        /// <summary>
        ///     The percent of each inventory entry that will be salvaged
        ///     upon destruction of the package.
        /// </summary>
        [DataField("percent", required: true)]
        public float Percent = 0.5f;

        [DataField("offset")]
        public float Offset { get; set; } = 0.5f;

        public void Execute(EntityUid owner, DestructibleSystem system, EntityUid? cause = null)
        {
            if (!system.EntityManager.TryGetComponent<VendingMachineRestockComponent>(owner, out var packagecomp) ||
                !system.EntityManager.TryGetComponent<TransformComponent>(owner, out var xform))
                return;

            var randomInventory = system.Random.Pick(packagecomp.CanRestock);

            if (!system.PrototypeManager.TryIndex(randomInventory, out VendingMachineInventoryPrototype? packPrototype))
                return;

            foreach (var (entityId, count) in packPrototype.StartingInventory)
            {
                var toSpawn = (int) Math.Round(count * Percent);

                if (toSpawn == 0) continue;

                if (EntityPrototypeHelpers.HasComponent<StackComponent>(entityId, system.PrototypeManager, system.ComponentFactory))
                {
                    var spawned = system.EntityManager.SpawnEntity(entityId, xform.Coordinates.Offset(system.Random.NextVector2(-Offset, Offset)));
                    system.StackSystem.SetCount(spawned, toSpawn);
                    system.EntityManager.GetComponent<TransformComponent>(spawned).LocalRotation = system.Random.NextAngle();
                }
                else
                {
                    for (var i = 0; i < toSpawn; i++)
                    {
                        var spawned = system.EntityManager.SpawnEntity(entityId, xform.Coordinates.Offset(system.Random.NextVector2(-Offset, Offset)));
                        system.EntityManager.GetComponent<TransformComponent>(spawned).LocalRotation = system.Random.NextAngle();
                    }
                }
            }
        }
    }
}