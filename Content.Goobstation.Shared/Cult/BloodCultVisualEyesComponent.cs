using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Cult;

/// <summary>
///     Gives cultists their glowing eyes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCultVisualEyesComponent : Component
{
    [DataField] public Color EyeColor = Color.Red;
    public Color? LastEyeColor;
}
