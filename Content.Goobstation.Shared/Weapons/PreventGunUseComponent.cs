using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons;

/// <summary>
/// This is used for anyone who has this component cant fire guns.
/// </summary>
[RegisterComponent, NetworkedComponent , AutoGenerateComponentState, AutoGenerateComponentPause  ]
public sealed partial class PreventGunUseComponent : Component
{
    [DataField, AutoNetworkedField ,AutoPausedField]
    public TimeSpan LastPopup = TimeSpan.Zero;

    [DataField, AutoNetworkedField ,AutoPausedField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3);
}
