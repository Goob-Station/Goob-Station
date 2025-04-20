namespace Content.Goobstation.Server.HisGrace;

[RegisterComponent]
public sealed partial class HisGraceUserComponent : Component
{
    /// <summary>
    ///  The speed multiplier of His Grace.
    /// </summary>
    [DataField]
    public float SpeedMultiplier = 1.2f;

}
