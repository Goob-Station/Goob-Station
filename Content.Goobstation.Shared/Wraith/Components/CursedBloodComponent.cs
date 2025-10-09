using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

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
    [DataField]
    public TimeSpan NextTickPuke = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will puke a lot of blood.
    /// </summary>
    [DataField]
    public TimeSpan NextTickBigPuke = TimeSpan.Zero;

    [DataField]
    public int BleedAmount = 15;

    /// <summary>
    /// How much blood they lose per tick.
    /// </summary>
    [DataField]
    public float BloodToSpill = 30f;
}
