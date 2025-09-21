using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedBloodComponent : Component
{
    /// <summary>
    /// How long before they puke blood.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillPuke = TimeSpan.FromSeconds(15);

    /// <summary>
    /// How long before they puke a lot of blood.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillBigPuke = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Next time at which they will puke blood.
    /// </summary>
    public TimeSpan NextTickPuke = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    public TimeSpan NextTickBigPuke = TimeSpan.Zero;

    /// <summary>
    /// Needed for death curse later. Gets set to true once max stacks are in.
    /// </summary>
    [DataField]
    public bool BloodCurseFullBloom;
}
