// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Botany;

/// <summary>
/// A barrel that slowly ferments the crops sealed inside it into drinks.
/// </summary>
[RegisterComponent]
public sealed partial class FermentingBarrelComponent : Component
{
    /// <summary>
    /// Container holding the crops waiting to ferment.
    /// </summary>
    [DataField]
    public string Container = "produce";

    /// <summary>
    /// Solution the drinks end up in.
    /// </summary>
    [DataField]
    public string Solution = "barrel";

    /// <summary>
    /// Components the barrel has while the lid is off. The lid is the barrel's item toggle.
    /// </summary>
    [DataField]
    public ComponentRegistry OpenComponents = new();

    /// <summary>
    /// Components the barrel has while the lid is on.
    /// </summary>
    [DataField]
    public ComponentRegistry ClosedComponents = new();

    /// <summary>
    /// How long each crop takes to ferment.
    /// </summary>
    [DataField]
    public TimeSpan TimePerCrop = TimeSpan.FromSeconds(2);

    /// <summary>
    /// What each kind of crop ferments into. Anything not listed becomes <see cref="DefaultDrink"/>.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, ProtoId<ReagentPrototype>> Drinks = new();

    /// <summary>
    /// The drink for crops missing from <see cref="Drinks"/>.
    /// </summary>
    [DataField]
    public ProtoId<ReagentPrototype> DefaultDrink = "FruitWine";

    /// <summary>
    /// The reagents in a crop that turn into its drink, and how much of each does. Everything else goes in as it is.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, float> Fermentable = new() { ["Nutriment"] = 1f, ["Vitamin"] = 1f };

    /// <summary>
    /// Crops that hold something else where tg's hold nutriment, like grains that hold flour, with what of theirs
    /// ferments in place of <see cref="Fermentable"/>.
    /// </summary>
    [DataField]
    public Dictionary<EntProtoId, Dictionary<ProtoId<ReagentPrototype>, float>> FermentFrom = new();

    /// <summary>
    /// Crops that won't ferment at all.
    /// </summary>
    [DataField]
    public List<EntProtoId> Unfermentable = new();

    /// <summary>
    /// Produce that isn't a food crop and doesn't go in, even though it can be eaten here.
    /// </summary>
    [DataField]
    public List<EntProtoId> NotCrops = new();

    /// <summary>
    /// Food crops that go in even though they can't be eaten here.
    /// </summary>
    [DataField]
    public List<EntProtoId> ExtraCrops = new();

    /// <summary>
    /// Bags that can tip their crops in.
    /// </summary>
    [DataField]
    public List<EntProtoId> Bags = new();
}
