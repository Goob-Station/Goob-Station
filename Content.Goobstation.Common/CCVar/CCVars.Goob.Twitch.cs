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
    /// Maximum number of characters accepted in a Twitch Bits station announcement.
    /// </summary>
    public static readonly CVarDef<int> TwitchBitsStationAnnouncementMaxLength =
        CVarDef.Create("twitch.bits.station_announcement_max_length", 160, CVar.SERVERONLY);

    /// <summary>
    /// Maximum number of characters accepted for target speech.
    /// </summary>
    public static readonly CVarDef<int> TwitchBitsCharacterSpeechMaxLength =
        CVarDef.Create("twitch.bits.character_speech_max_length", 120, CVar.SERVERONLY);

    /// <summary>
    /// Twitch account login used by the server to read channel chat.
    /// </summary>
    public static readonly CVarDef<string> TwitchChatBotLogin =
        CVarDef.Create("twitch.chat.bot_login", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// OAuth user access token with the chat:read scope for the configured Twitch chat account.
    /// </summary>
    public static readonly CVarDef<string> TwitchChatOauthToken =
        CVarDef.Create("twitch.chat.oauth_token", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Client ID of the Twitch application used by the chat bot account.
    /// </summary>
    public static readonly CVarDef<string> TwitchChatClientId =
        CVarDef.Create("twitch.chat.client_id", string.Empty, CVar.SERVERONLY);

    /// <summary>
    /// Client secret of the Twitch application used to refresh the chat bot token.
    /// </summary>
    public static readonly CVarDef<string> TwitchChatClientSecret =
        CVarDef.Create("twitch.chat.client_secret", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// OAuth refresh token for the Twitch chat bot account.
    /// </summary>
    public static readonly CVarDef<string> TwitchChatRefreshToken =
        CVarDef.Create("twitch.chat.refresh_token", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Number of seconds a Chat Critter session remains active.
    /// </summary>
    public static readonly CVarDef<int> TwitchChatCritterDuration =
        CVarDef.Create("twitch.chat_critter.duration", 300, CVar.SERVERONLY);
}
