using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for the Hypnosis ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingHypnosisComponent : Component
{
    [DataField]
    public EntProtoId HypnosisAction = "ActionHynosis";
}
