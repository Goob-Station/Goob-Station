// SPDX-FileCopyrightText: 2026 White Dream Project contributors
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.Construction;
using Content.Shared._White.RadialSelector;
using Content.Shared.BloodCult.Components;
using Content.Shared.Construction.Prototypes;
using Robust.Client.Placement;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client.BloodCult;

public sealed class CultConstructionMenuSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlacementManager _placement = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    [Dependency] private readonly ConstructionSystem _construction = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CultConstructionMenuComponent, RadialSelectorSelectedMessage>(OnSelected);
    }

    private void OnSelected(Entity<CultConstructionMenuComponent> menu, ref RadialSelectorSelectedMessage args)
    {
        var selectedItem = args.SelectedItem;
        if (!_timing.IsFirstTimePredicted ||
            !menu.Comp.Entries.Any(entry => entry.Prototype == selectedItem) ||
            !_prototype.TryIndex(selectedItem, out ConstructionPrototype? construction))
            return;

        if (construction.Type == ConstructionType.Item)
        {
            _construction.TryStartItemConstruction(construction.ID);
            return;
        }

        var hijack = new ConstructionPlacementHijack(_construction, construction);
        _placement.BeginPlacing(
            new PlacementInformation
            {
                IsTile = false,
                PlacementOption = construction.PlacementMode,
            },
            hijack);
    }
}
