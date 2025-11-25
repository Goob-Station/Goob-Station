using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Slasher.Objectives;

/// <summary>
/// Just absorb 10 souls
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherAbsorbSoulsConditionComponent : Component
{
    /// <summary>
    /// Souls absorbed so far for this objective.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int Absorbed;
}
