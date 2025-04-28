using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Hologram;

[RegisterComponent, NetworkedComponent]
public sealed partial class HologramComponent : Component
{
    /// <summary>
    /// Name of the shader to use
    /// </summary>
    [DataField]
    public string ShaderName = "Hologram";

    /// <summary>
    /// The primary color
    /// </summary>
    [DataField]
    public Color Color1 = Color.FromHex("#65B8E2");

    /// <summary>
    /// The secondary color
    /// </summary>
    [DataField]
    public Color Color2 = Color.FromHex("#3A6981");

    /// <summary>
    /// The shared color alpha
    /// </summary>
    [DataField]
    public float Alpha = 0.9f;

    /// <summary>
    /// The color brightness
    /// </summary>
    [DataField]
    public float Intensity = 2f;

    /// <summary>
    /// The scroll rate of the hologram shader
    /// </summary>
    [DataField]
    public float ScrollRate = 0.125f;
}
