namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingLightningStormComponent : Component
{
    [DataField]
    public TimeSpan TimeBeforeActivation = TimeSpan.FromSeconds(10);

    [DataField]
    public float Range = 12f;

    [DataField]
    public int BoltCount = 15;

    [DataField]
    public string LightningProto = "HyperchargedLightning";
}
