// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// An automated FitsInDispenser slot, whose solution can be used by liquid pumps.
/// </summary>
public sealed partial class AutomatedBeakerSlot : AutomationSlot
{
    [DataField]
    public string SlotName = "beakerSlot";

    private ItemSlotsSystem _slots;
    private SharedSolutionContainerSystem _solution;

    public override void Initialize()
    {
        base.Initialize();

        _slots = EntMan.System<ItemSlotsSystem>();
        _solution = EntMan.System<SharedSolutionContainerSystem>();
    }

    public override Entity<SolutionComponent>? GetSolution()
    {
        if (_slots.GetItemOrNull(Owner, SlotName) is not {} beaker)
            return null;

        _solution.TryGetFitsInDispenser(beaker, out var solution, out _);
        return solution;
    }
}
