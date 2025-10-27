namespace Content.Shared._Lavaland.Interaction;

/// <summary>
/// This is used for itms that cant be let go
/// </summary>
[RegisterComponent]
public sealed partial class UndroppableComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public TimeSpan LastPopup = TimeSpan.Zero;

    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3);
}
