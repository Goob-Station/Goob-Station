// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles; // Goob: Ported from DeltaV - Species specific trait support.

// Pirate start: port and modified DV traits system
using Content.Shared._Pirate.Traits.Conditions;
using Content.Shared._Pirate.Traits.Effects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
// Pirate end: port and modified DV traits system

namespace Content.Shared.Traits;

/// <summary>
/// Describes a trait.
/// </summary>
[Prototype]
public sealed partial class TraitPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The name of this trait.
    /// </summary>
    [DataField]
    public LocId Name { get; private set; } = string.Empty;

    /// <summary>
    /// The description of this trait.
    /// </summary>
    [DataField]
    public LocId? Description { get; private set; }

    /// <summary>
    /// Don't apply this trait to entities this whitelist IS NOT valid for.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Don't apply this trait to entities this whitelist IS valid for. (hence, a blacklist)
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    // Pirate start: port and modified DV traits system
    /// <summary>
    /// Pirate port: Modernized - List of conditions to meet for availability.
    /// </summary>
    [DataField("conditions")]
    public List<BaseTraitCondition> Conditions { get; set; } = new();

    /// <summary>
    /// Pirate port: Modernized - Effects applied when the trait is granted.
    /// </summary>
    [DataField("effects")]
    public List<BaseTraitEffect> Effects { get; set; } = new();

    /// <summary>
    /// Pirate port: Modernized - Other traits that are mutually exclusive with this one.
    /// </summary>
    [DataField("conflicts")]
    public List<ProtoId<TraitPrototype>> Conflicts { get; set; } = new();

    /// <summary>
    /// The components that get added to the player, when they pick this trait.
    /// NOTE: When implementing a new trait, it's preferable to add it as a status effect instead if possible.
    /// </summary>
    [DataField("components")]
    public ComponentRegistry Components { get; set; } = new();

    /// <summary>
    /// Special effects applied to the player who takes this Trait.
    /// </summary>
    [DataField(serverOnly: true)]
    public List<JobSpecial> Specials { get; private set; } = new();
    // Pirate end: port and modified DV traits system

    /// <summary>
    /// Gear that is given to the player, when they pick this trait.
    /// </summary>
    [DataField]
    public EntProtoId? TraitGear;

    /// <summary>
    /// Trait Price. If negative number, points will be added.
    /// </summary>
    [DataField]
    public int Cost = 0;

    /// <summary>
    /// Adds a trait to a category, allowing you to limit the selection of some traits to the settings of that category.
    /// </summary>
    [DataField]
    public ProtoId<TraitCategoryPrototype>? Category;

    // Pirate start: port and modified DV traits system
    /// <summary>
    /// Goob: Ported from DeltaV - Hides traits from specific species
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> ExcludedSpecies = new();

    // Goob: Only shows traits to specific species
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>> IncludedSpecies = new();
    // Pirate end: port and modified DV traits system

    // Einstein Engines - Language begin (remove this if trait system refactor)
    /// <summary>
    ///     The list of all Spoken Languages that this trait adds.
    /// </summary>
    [DataField]
    public List<string>? LanguagesSpoken { get; private set; } = default!;

    /// <summary>
    ///     The list of all Understood Languages that this trait adds.
    /// </summary>
    [DataField]
    public List<string>? LanguagesUnderstood { get; private set; } = default!;

    /// <summary>
    ///     The list of all Spoken Languages that this trait removes.
    /// </summary>
    [DataField]
    public List<string>? RemoveLanguagesSpoken { get; private set; } = default!;

    /// <summary>
    ///     The list of all Understood Languages that this trait removes.
    /// </summary>
    [DataField]
    public List<string>? RemoveLanguagesUnderstood { get; private set; } = default!;
    // Einstein Engines - Language end
}
