using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

public sealed partial class GoobCVars
{
    /// <summary>
    /// Client ID of the Twitch Extension associated with this integration.
    /// </summary>
    public static readonly CVarDef<string> TwitchExtensionClientId =
        CVarDef.Create("twitch.extension.client_id", "nockr8c6gn4p37cvim2ivwsaa0mzdv", CVar.SERVERONLY);

    /// <summary>
    /// Twitch user ID that owns the Extension and signs EBS API requests.
    /// </summary>
    public static readonly CVarDef<string> TwitchExtensionOwnerUserId =
        CVarDef.Create("twitch.extension.owner_user_id", "40710601", CVar.SERVERONLY);

    /// <summary>
    /// Twitch channel this SS14 server accepts Extension requests for.
    /// </summary>
    public static readonly CVarDef<string> TwitchExtensionChannelId =
        CVarDef.Create("twitch.extension.channel_id", "40710601", CVar.SERVERONLY);

    /// <summary>
    /// Base64-encoded shared secret from the Twitch Extension settings.
    /// </summary>
    public static readonly CVarDef<string> TwitchExtensionSecret =
        CVarDef.Create("twitch.extension.secret", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Enables the HTTP API used by the Twitch integration.
    /// The Robust status host must also be enabled for the API to be reachable.
    /// </summary>
    public static readonly CVarDef<bool> TwitchApiEnabled =
        CVarDef.Create("twitch.api.enabled", false, CVar.SERVERONLY);

    /// <summary>
    /// Bearer token used by trusted server-to-server Twitch API routes.
    /// This is not the Twitch Extension shared secret.
    /// </summary>
    public static readonly CVarDef<string> TwitchApiToken =
        CVarDef.Create("twitch.api.token", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Comma-separated list of origins allowed to call the API from a browser.
    /// Twitch Extension origins should be listed explicitly, for example
    /// https://extension-id.ext-twitch.tv.
    /// </summary>
    public static readonly CVarDef<string> TwitchApiAllowedOrigins =
        CVarDef.Create(
            "twitch.api.allowed_origins",
            "https://nockr8c6gn4p37cvim2ivwsaa0mzdv.ext-twitch.tv",
            CVar.SERVERONLY);

    /// <summary>
    /// Maximum accepted request body size in bytes.
    /// </summary>
    public static readonly CVarDef<int> TwitchApiMaxRequestBodySize =
        CVarDef.Create("twitch.api.max_request_body_size", 64 * 1024, CVar.SERVERONLY);

    /// <summary>
    /// Enables paid Twitch Bits actions.
    /// </summary>
    public static readonly CVarDef<bool> TwitchBitsEnabled =
        CVarDef.Create("twitch.bits.enabled", true, CVar.SERVERONLY);

    /// <summary>
    /// SS14 username whose current in-round character receives Twitch Bits actions.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsTargetUsername =
        CVarDef.Create("twitch.bits.target_username", "GoobStation", CVar.SERVERONLY);

    /// <summary>
    /// Twitch Bits catalog SKU for inverting the target's controls for 60 seconds.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsInvertControlsSku =
        CVarDef.Create("twitch.bits.sku.invert_controls", "ss14-invert-controls-60", CVar.SERVERONLY);

    /// <summary>
    /// Twitch Bits catalog SKU for dropping the target's active-hand item.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsDropHeldSku =
        CVarDef.Create("twitch.bits.sku.drop_held", "ss14-drop-held", CVar.SERVERONLY);

    /// <summary>
    /// Twitch Bits catalog SKU for dropping all of the target's equipment and held items.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsDropAllSku =
        CVarDef.Create("twitch.bits.sku.drop_all", "ss14-drop-all", CVar.SERVERONLY);

    /// <summary>
    /// Twitch Bits catalog SKU for arming an available nuclear device.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsArmNukeSku =
        CVarDef.Create("twitch.bits.sku.arm_nuke", "ss14-arm-nuke", CVar.SERVERONLY);

    /// <summary>
    /// Twitch Bits catalog SKU for sending a viewer-authored station announcement.
    /// </summary>
    public static readonly CVarDef<string> TwitchBitsStationAnnouncementSku =
        CVarDef.Create("twitch.bits.sku.station_announcement", "ss14-station-announcement", CVar.SERVERONLY);

    /// <summary>
    /// Maximum number of characters accepted in a Twitch Bits station announcement.
    /// </summary>
    public static readonly CVarDef<int> TwitchBitsStationAnnouncementMaxLength =
        CVarDef.Create("twitch.bits.station_announcement_max_length", 160, CVar.SERVERONLY);
}
