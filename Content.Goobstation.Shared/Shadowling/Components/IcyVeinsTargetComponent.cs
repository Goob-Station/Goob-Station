using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling.Components;

/// <summary>
/// Marks target as affected by Icy Veins and applies its effects
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class IcyVeinsTargetComponent : Component
{
    [DataField]
    public float DecreaseTempByValue = 5.0f;

    [DataField]
    public float MinTargetTemperature = 200.0f;

    [DataField]
    public float TimeUntilNextDecrease = 0.6f;

    [DataField]
    public float Timer = 0.6f;
}
