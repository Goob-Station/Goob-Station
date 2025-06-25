namespace Content.Shared._EE.Shadowling;


/// <summary>
/// This is used for Ascendance debug
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingAscendanceComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(20);

    [DataField]
    public string EggProto = "SlingEggAscension";
}
