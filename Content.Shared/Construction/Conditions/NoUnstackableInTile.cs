// SPDX-FileCopyrightText: 2023 Ben <50087092+benev0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 BenOwnby <ownbyb@appstate.edu>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Map;

namespace Content.Shared.Construction.Conditions;

/// <summary>
///   Check for "Unstackable" condition commonly used by atmos devices and others which otherwise don't check on
///   collisions with other items.
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class NoUnstackableInTile : IConstructionCondition
{
    public const string GuidebookString = "construction-step-condition-no-unstackable-in-tile";
    public bool Condition(EntityUid user, EntityCoordinates location, Direction direction)
    {
        var sysMan = IoCManager.Resolve<IEntitySystemManager>();
        var anchorable = sysMan.GetEntitySystem<AnchorableSystem>();

        return !anchorable.AnyUnstackablesAnchoredAt(location);
    }

    public ConstructionGuideEntry GenerateGuideEntry()
    {
        return new ConstructionGuideEntry
        {
            Localization = GuidebookString
        };
    }
}