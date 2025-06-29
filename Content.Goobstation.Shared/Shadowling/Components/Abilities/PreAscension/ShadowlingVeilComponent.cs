using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;

/// <summary>
/// This is used for Veil Ability
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingVeilComponent : Component
{
    [DataField]
    public EntProtoId ActionGlare = "ActionVeil";

    [DataField]
    public float Range = 6f;
}
