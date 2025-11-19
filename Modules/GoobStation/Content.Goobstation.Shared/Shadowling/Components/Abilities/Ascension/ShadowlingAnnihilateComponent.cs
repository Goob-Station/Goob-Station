using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.Ascension;

/// <summary>
/// This is used for the Annihilate ability.
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingAnnihilateComponent : Component
{
    [DataField]
    public EntProtoId ActionId = "ActionAnnihilate";

    [DataField]
    public EntityUid? ActionEnt;
}
