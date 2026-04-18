// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.EntityConditions;
using Content.Shared.Inventory;
using Content.Shared.Chemistry.Components.SolutionManager;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects.EffectConditions;

public sealed partial class HasComponentOnEquipmentConditionSystem
    : EntityConditionSystem<SolutionContainerManagerComponent, HasComponentOnEquipmentCondition>
{
    protected override void Condition(Entity<SolutionContainerManagerComponent> entity, ref EntityConditionEvent<HasComponentOnEquipmentCondition> args)
    {
        if (args.Condition.Components.Count == 0)
        {
            args.Result = args.Condition.Invert;
            return;
        }

        if (TryComp<InventoryComponent>(entity.Owner, out var inv) &&
            EntityManager.System<InventorySystem>().TryGetContainerSlotEnumerator(entity.Owner, out var enumerator, SlotFlags.WITHOUT_POCKET))
        {
            while (enumerator.NextItem(out var item))
            {
                if (!args.Condition.Components.Any(comp =>
                        HasComp(item, comp.Value.Component.GetType())))
                    continue;
                args.Result = !args.Condition.Invert;
                return;
            }
        }

        args.Result = args.Condition.Invert;
    }
}

public sealed partial class HasComponentOnEquipmentCondition : EntityConditionBase<HasComponentOnEquipmentCondition>
{
    [DataField(required: true)]
    public ComponentRegistry Components = default!;

    [DataField]
    public bool Invert = false;

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        // Same reasoning as before
        return "TODO";
    }
}
