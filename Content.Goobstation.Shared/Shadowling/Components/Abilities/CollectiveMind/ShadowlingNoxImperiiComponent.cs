using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Shadowling.Components.Abilities.CollectiveMind;

/// <summary>
/// This is used for the Nox Imperii ability.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ShadowlingNoxImperiiComponent : Component
{
    [DataField]
    public EntProtoId ActionNoxImperii = "ActionNoxImperii";

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);

    [DataField]
    public float Radius = 1.5f;

    [DataField]
    public float LightEnergy = 5f;
}
