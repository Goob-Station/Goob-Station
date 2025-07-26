using Robust.Shared.GameStates;


namespace Content.Goobstation.Shared.Shadowling.Components;


/// <summary>
/// This is used for the Anti Mind Control device
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AntiMindControlItemComponent : Component
{
    /// <summary>
    /// Indicates how long the duration of the item use lasts for.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Indicates how many charges the device has.
    /// </summary>
    [DataField]
    public int MaxCharges;

    /// <summary>
    /// Indicates the current charges of the device.
    /// </summary>
    [DataField]
    public int CurrentCharges;
}
