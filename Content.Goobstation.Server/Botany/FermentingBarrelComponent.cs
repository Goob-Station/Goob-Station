// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Botany;

/// <summary>
/// A barrel that slowly ferments the crops sealed inside it into drinks.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
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
    /// Whether the lid is off. Crops only go in while it's open and only ferment while it's shut.
    /// </summary>
    [DataField]
    public bool Open;

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
    /// The reagents in a crop that turn into its drink. Everything else goes in as it is.
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>> Fermentable = new() { "Nutriment", "Vitamin", "Flour", "Oats", "Rice", "Sugar" };

    /// <summary>
    /// When the next crop ferments.
    /// </summary>
    [ViewVariables, AutoPausedField]
    public TimeSpan NextFerment;
}
