// SPDX-FileCopyrightText: 2024 Kevin Zheng <kevinz5000@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects.Effects;

/// <summary>
/// A reaction effect that spawns a PrototypeID in the entity's Slot, and attempts to consume the reagent if EntityEffectReagentArgs.
/// Used to implement the water droplet effect for arachnids.
/// </summary>
public sealed partial class WearableReaction : EntityEffect
{
    /// <summary>
    /// Minimum quantity of reagent required to trigger this effect.
    /// Only used with EntityEffectReagentArgs.
    /// </summary>
    [DataField]
    public float AmountThreshold = 1f;

    /// <summary>
    /// Slot to spawn the item into.
    /// </summary>
    [DataField(required: true)]
    public string Slot;

    /// <summary>
    /// Prototype ID of item to spawn.
    /// </summary>
    [DataField(required: true)]
    public string PrototypeID;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        // SpawnItemInSlot returns false if slot is already occupied
        if (args.EntityManager.System<InventorySystem>().SpawnItemInSlot(args.TargetEntity, Slot, PrototypeID))
        {
            if (args is EntityEffectReagentArgs reagentArgs)
            {
                if (reagentArgs.Reagent == null || reagentArgs.Quantity < AmountThreshold)
                    return;
                reagentArgs.Source?.RemoveReagent(reagentArgs.Reagent.ID, AmountThreshold);
            }
        }
    }
}