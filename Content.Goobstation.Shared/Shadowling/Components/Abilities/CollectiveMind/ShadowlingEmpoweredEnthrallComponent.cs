using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for Empowered Enthrall ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingEmpoweredEnthrallComponent : Component
{
    /// <summary>
    /// The duration it takes to complete the enthrallment process.
    /// </summary>
    [DataField]
    public TimeSpan EnthrallTime = TimeSpan.FromSeconds(1.2);

    [DataField]
    public EntProtoId EnthrallComponents = "ThrallAbilities";

    [DataField]
    public EntProtoId ActionId = "ActionEmpoweredEnthrall";

    [DataField]
    public EntityUid? ActionEnt;
}
