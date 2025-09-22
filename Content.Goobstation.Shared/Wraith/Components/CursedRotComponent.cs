using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedRotComponent : Component
{
    /// <summary>
    /// How long before they puke blood.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillPuke = TimeSpan.FromSeconds(10);

    /// <summary>
    /// How long before they puke a lot of blood.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillCough = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Next time at which they will puke blood.
    /// </summary>
    public TimeSpan NextTickPuke = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    public TimeSpan NextTickCough = TimeSpan.Zero;
}
