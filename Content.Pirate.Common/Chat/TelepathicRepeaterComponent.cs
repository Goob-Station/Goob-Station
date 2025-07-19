using Robust.Shared.GameStates;

namespace Content.Pirate.Common.Chat;

/// <summary>
/// Component for telepathic repeater functionality
/// This is placed in Common so Core modules can access it
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TelepathicRepeaterComponent : Component
{
    /// <summary>
    /// Whether this repeater is currently active
    /// </summary>
    [DataField]
    public bool Active = true;

    /// <summary>
    /// Range of the telepathic repeater
    /// </summary>
    [DataField]
    public float Range = 50f;
}
