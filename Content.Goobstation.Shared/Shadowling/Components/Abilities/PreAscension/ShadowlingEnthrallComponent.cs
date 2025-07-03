using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for the Basic Enthrall Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingEnthrallComponent : Component
{
    [DataField]
    public EntProtoId ActionEnthrall = "ActionEnthrall";

    /// <summary>
    /// Indicates how long the enthrallment process takes.
    /// </summary>
    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(5);
}
