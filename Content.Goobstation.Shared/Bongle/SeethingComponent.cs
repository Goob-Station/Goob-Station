using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Bongle;

/// <summary>
/// Red overlay on screen, play heartbeat sound, popup every X amount of time.
/// Also steals drone code for overlay if you set a component, so it will hide every living entity except the specified component having entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SeethingComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(30);

    [AutoPausedField, ViewVariables]
    public TimeSpan ExpireTime;

    [DataField]
    public TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(3);

    [AutoPausedField, ViewVariables]
    public TimeSpan NextHeartbeat;

    [DataField]
    public SoundSpecifier? HeartbeatSound;

    [AutoPausedField, DataField]
    public TimeSpan PopupInterval = TimeSpan.FromSeconds(10);

    [ViewVariables]
    public TimeSpan NextPopup;

    [DataField]
    public LocId? Popup;

    [DataField, AutoNetworkedField]
    public Color OverlayColor = new(1f, 0f, 0f, 0.1f);

    /// <summary>
    /// If null, no overlay on top of stuff, if not null then all living beings except the whitelist will be overlayed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityWhitelist? VisibleWhitelist;

    [DataField, AutoNetworkedField]
    public EntityWhitelist? VisibleBlacklist;
}
