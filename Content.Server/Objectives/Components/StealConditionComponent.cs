// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Server.Objectives.Systems;
using Content.Shared.Objectives;
using Robust.Shared.Prototypes;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that you steal a certain item (or several)
/// </summary>
[RegisterComponent, Access(typeof(StealConditionSystem))]
public sealed partial class StealConditionComponent : Component
{
    /// <summary>
    /// A group of items to be stolen
    /// </summary>
    [DataField(required: true)]
    public ProtoId<StealTargetGroupPrototype> StealGroup;

    /// <summary>
    /// When enabled, disables generation of this target if there is no entity on the map (disable for objects that can be created mid-round).
    /// </summary>
    [DataField]
    public bool VerifyMapExistence = true;

    /// <summary>
    /// If true, counts objects that are close to steal areas.
    /// </summary>
    [DataField]
    public bool CheckStealAreas = false;

    /// <summary>
    /// If the target may be alive but has died, it will not be counted
    /// </summary>
    [DataField]
    public bool CheckAlive = false;

    /// <summary>
    /// The minimum number of items you need to steal to fulfill a objective
    /// </summary>
    [DataField]
    public int MinCollectionSize = 1;

    /// <summary>
    /// The maximum number of items you need to steal to fulfill a objective
    /// </summary>
    [DataField]
    public int MaxCollectionSize = 1;

    /// <summary>
    /// Target collection size after calculation
    /// </summary>
    [DataField]
    public int CollectionSize;

    /// <summary>
    /// Help newer players by saying e.g. "steal the chief engineer's advanced magboots"
    /// instead of "steal advanced magboots. Should be a loc string.
    /// </summary>
    [DataField("owner")]
    public string? OwnerText;

    // All this need to be loc string
    [DataField(required: true)]
    public LocId ObjectiveText;
    [DataField(required: true)]
    public LocId ObjectiveNoOwnerText;
    [DataField(required: true)]
    public LocId DescriptionText;
    [DataField(required: true)]
    public LocId DescriptionMultiplyText;
}