// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Goobstation.Shared._Trauma.Ranching.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class EggFertilizerComponent : Component
{
    /// <summary>
    /// How long the doafter takes
    /// </summary>
    [DataField]
    public TimeSpan DoAfter = TimeSpan.FromSeconds(15);

    /// <summary>
    /// If SpecialReplacement is not null the egg will hatch whatever entity SpecialReplacement is instead of the default one
    /// </summary>
    [DataField]
    public EntProtoId? SpecialReplacement;

    /// <summary>
    /// What egg is required for the SpecialReplacement to happen, if null it happens for all eggs
    /// </summary>
    [DataField]
    public EntProtoId? SpecialReplacementRequiredEgg;
}
