using Robust.Shared.Prototypes;

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

    [DataField]
    public EntProtoId BombProto = "PipeBomb";
}
