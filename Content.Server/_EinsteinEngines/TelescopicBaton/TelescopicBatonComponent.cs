namespace Content.Server._EinsteinEngines.TelescopicBaton;

[RegisterComponent]
public sealed partial class TelescopicBatonComponent : Component
{
    [DataField]
    public bool CanDropItems; // Goob edit

    [DataField]
    public bool AlwaysDropItems; // Goobstation

    /// <summary>
    ///     The amount of time during which the baton will be able to knockdown someone after activating it.
    /// </summary>
    [DataField]
    public TimeSpan AttackTimeframe = TimeSpan.FromSeconds(1.8f); // Goob edit

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeframeAccumulator = TimeSpan.FromSeconds(0);
}
