// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Audio;
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
    /// Added while the lid is off and removed when it's shut: pouring in, and seeing what's inside.
    /// </summary>
    [DataField]
    public ComponentRegistry OpenComponents = new();

    /// <summary>
    /// Added while the lid is shut and removed when it's off: the tap.
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
    /// Crops that won't ferment at all.
    /// </summary>
    [DataField]
    public List<EntProtoId> Unfermentable = new();

    /// <summary>
    /// The reagents in a crop that turn into its drink. Everything else goes in as it is.
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>> Fermentable = new() { "Nutriment", "Vitamin", "Flour", "Oats", "Rice", "Sugar" };

    /// <summary>
    /// Played when the lid comes off.
    /// </summary>
    [DataField]
    public SoundSpecifier? OpenSound;

    /// <summary>
    /// Played when the lid goes on.
    /// </summary>
    [DataField]
    public SoundSpecifier? CloseSound;

    /// <summary>
    /// When the next crop ferments.
    /// </summary>
    [ViewVariables, AutoPausedField]
    public TimeSpan NextFerment;
}
