namespace Content.Goobstation.Server.MisandryBox.TideLimiter;

/// <summary>
/// Used on station entities to limit amount of "tider" jobs available per "security" jobs.
/// </summary>
[RegisterComponent]
public sealed partial class TideLimitedComponent : Component
{
    /// <summary>
    /// Are we currently limiting slots
    /// </summary>
    public bool Active = false;

    /// <summary>
    /// How much bound slots should open per taken security
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int Ratio = 2;

    /// <summary>
    /// Role id that is bound by this system
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public string Bound = "Passenger";

    [ViewVariables(VVAccess.ReadOnly)]
    public int BoundCount = 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public int SecurityCount = 0;
}
