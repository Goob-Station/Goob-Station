using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Shadowling.Components;

/// <summary>
/// This is used after you get anti-mind controlled. It takes a little longer to get thralled again.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EnthrallResistanceComponent : Component
{
    /// <summary>
    /// How much extra time required to enthrall again.
    /// </summary>
    [DataField]
    public TimeSpan ExtraTime = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How much extra time gets added with each de-thrall.
    /// </summary>
    [DataField]
    public TimeSpan ExtraTimeUpdate = TimeSpan.FromSeconds(1);
}
