namespace Content.Goobstation.Server.MalfunctionAi;

/// <summary>
/// Temporarily added to an air vent during a Malfunction AI plasma flood.
/// While present the vent adds plasma to its tile every tick, until <see cref="EndTime"/>.
/// </summary>
[RegisterComponent]
public sealed partial class MalfPlasmaVentComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public float MolesPerSecond = 5f;
}
