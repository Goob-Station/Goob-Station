namespace Content.Goobstation.Shared.Weapons;

/// <summary>
/// This is used for anyone who has this component cant fire guns.
/// </summary>
[RegisterComponent]
public sealed partial class PreventGunUseComponent : Component
{

    [DataField]
    public TimeSpan LastPopup = TimeSpan.Zero;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3);
}
