using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for the Basic Enthrall Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingEnthrallComponent : Component
{
    /// <summary>
    /// Indicates how long the enthrallment process takes.
    /// </summary>
    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(5);

    [DataField]
    public EntProtoId EnthrallComponents = "ThrallAbilities";

    [DataField]
    public EntProtoId ActionId = "ActionEnthrall";

    [DataField]
    public EntityUid? ActionEnt;
}
