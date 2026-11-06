using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Research;

/// <summary>
/// Makes toggeable items a one time use. The timer starts on the first activation, and once it runs out the item is forced off and can never be activated again.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class ToggleExpiryComponent : Component
{
    /// <summary>
    /// How long after the first activation the item expires.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Played on expiring.
    /// </summary>
    [DataField]
    public SoundSpecifier? ExpireSound;

    [DataField]
    public LocId ExpiredPopup = "toggle-expired";

    /// <summary>
    /// When the item expires.
    /// </summary>
    [DataField, AutoNetworkedField, AutoPausedField]
    public TimeSpan? ExpireTime;

    /// <summary>
    /// Once true the item can no longer be activated.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Expired;
}
