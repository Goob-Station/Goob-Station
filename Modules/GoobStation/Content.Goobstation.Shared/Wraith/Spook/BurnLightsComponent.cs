using Content.Shared.Destructible.Thresholds;

namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent]
public sealed partial class BurnLightsComponent : Component
{
    /// <summary>
    /// Search radius of lights
    /// </summary>
    [DataField]
    public float SearchRadius = 15f;

    /// <summary>
    ///  How many lights to burn
    /// </summary>
    [DataField]
    public int MaxBurnLights = 4;

    /// <summary>
    /// Range, inside which all entities going be set on fire.
    /// </summary>
    [DataField]
    public float Range = 4f;

    /// <summary>
    /// Amount of fire stacks to apply
    /// </summary>
    [DataField]
    public MinMax FireStack = new(1, 3);
}
