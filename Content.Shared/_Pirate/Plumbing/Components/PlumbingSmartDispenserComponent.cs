using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._Pirate.Plumbing.Components;

/// <summary>
/// A plumbing-connected smart dispenser that stores reagents pulled from the network,
/// supports held container dispensing and label matching dispensing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlumbingSmartDispenserComponent : Component
{
    /// <summary>
    /// The solution container name for the dispenser's reagent storage.
    /// </summary>
    [DataField]
    public string SolutionName = "fridge";

    /// <summary>
    /// Maximum amount of any single reagent the dispenser can store.
    /// </summary>
    [DataField]
    public FixedPoint2 MaxPerReagent = FixedPoint2.New(200);
}
