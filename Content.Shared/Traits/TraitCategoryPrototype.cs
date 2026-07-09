// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Maths; // Pirate: port and modified DV traits system
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;

/// <summary>
/// Traits category with general settings. Allows you to limit the number of taken traits in one category
/// </summary>
[Prototype]
public sealed partial class TraitCategoryPrototype : IPrototype
{
    public const string Default = "Default";

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Name of the trait category displayed in the UI
    /// </summary>
    [DataField]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    ///     The maximum number of traits that can be taken in this category.
    /// </summary>
    [DataField]
    public int? MaxTraitPoints;

    // Pirate start: port and modified DV traits system
    [DataField] public int? MaxTraits;
    [DataField] public int? MaxPoints;
    [DataField] public bool DefaultExpanded = true;
    [DataField] public Color? AccentColor;

    [DataField] public int Priority = 0;
    // Pirate end: port and modified DV traits system
}