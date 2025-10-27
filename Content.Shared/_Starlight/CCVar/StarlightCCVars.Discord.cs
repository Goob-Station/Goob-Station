using Robust.Shared.Configuration;

namespace Content.Shared.Starlight.CCVar;
public sealed partial class StarlightCCVars
{
    /// <summary>
    /// Discord oAuth
    /// </summary>

    public static readonly CVarDef<string> DiscordCallback =
        CVarDef.Create("discord.callback", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> Secret =
        CVarDef.Create("discord.secret", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);
        
        
    /// <summary>
    /// Discord Webhooks
    /// </summary>
    
    public static readonly CVarDef<string> DiscordBanWebhook =
        CVarDef.Create("discord.ban_webhook", string.Empty, CVar.SERVERONLY);
}
