namespace Content.Server.TelescopicBaton;

[RegisterComponent]
public sealed partial class TelescopicBatonComponent : Component
{
    [DataField]
    public bool CanKnockDown;

    /// <summary>
    ///     The amount of time during which the baton will be able to knockdown someone after activating it.
    /// </summary>
    [DataField]
    public TimeSpan AttackTimeframe = TimeSpan.FromSeconds(1.5f);

    [ViewVariables(VVAccess.ReadOnly)]
    public TimeSpan TimeframeAccumulator = TimeSpan.FromSeconds(0);
}
