namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent]
public sealed partial class BurnLightsComponent : Component
{
    [DataField]
    public float SearchRadius = 10f;

    [DataField]
    public int MaxBurnLights = 4;
}
