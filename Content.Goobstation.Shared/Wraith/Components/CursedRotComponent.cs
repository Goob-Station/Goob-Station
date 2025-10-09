using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CursedRotComponent : Component
{
    /// <summary>
    /// How long before they puke.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillPuke = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long before they cough.
    /// </summary>
    [DataField]
    public TimeSpan TimeTillCough = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Next time at which they will puke.
    /// </summary>
    public TimeSpan NextTickPuke = TimeSpan.Zero;

    /// <summary>
    /// Next time at which they will cough.
    /// </summary>
    public TimeSpan NextTickCough = TimeSpan.Zero;
}
