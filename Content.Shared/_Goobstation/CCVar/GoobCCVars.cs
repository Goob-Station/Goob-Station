using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CCVars;

[CVarDefs]
public sealed class GoobCCVars : Robust.Shared.CVars
{
    /// <summary>
    ///     Indicates how much players are required for the round to be considered lowpop.
    ///     Used for dynamic gamemode.
    /// </summary>
    public static readonly CVarDef<float> LowpopThreshold =
        CVarDef.Create("game.players.lowpop_threshold", 15f, CVar.SERVERONLY);

    /// <summary>
    ///     Indicates how much players are required for the round to be considered highpop.
    ///     Used for dynamic gamemode.
    /// </summary>
    public static readonly CVarDef<float> HighpopThreshold =
        CVarDef.Create("game.players.highpop_threshold", 50f, CVar.SERVERONLY);

    /// <summary>
    ///     Is ore silo enabled.
    /// </summary>
    public static readonly CVarDef<bool> SiloEnabled =
        CVarDef.Create("game.silo_enabled", true, CVar.SERVER | CVar.REPLICATED);

    /*
        * Announcers
        */

    /// <summary>
    ///     Weighted list of announcers to choose from
    /// </summary>
    public static readonly CVarDef<string> AnnouncerList =
        CVarDef.Create("announcer.list", "RandomAnnouncers", CVar.REPLICATED);

    /// <summary>
    ///     Optionally force set an announcer
    /// </summary>
    public static readonly CVarDef<string> Announcer =
        CVarDef.Create("announcer.announcer", "", CVar.SERVERONLY);

    /// <summary>
    ///     Optionally blacklist announcers
    ///     List of IDs separated by commas
    /// </summary>
    public static readonly CVarDef<string> AnnouncerBlacklist =
        CVarDef.Create("announcer.blacklist", "", CVar.SERVERONLY);

    /// <summary>
    ///     Changes how loud the announcers are for the client
    /// </summary>
    public static readonly CVarDef<float> AnnouncerVolume =
        CVarDef.Create("announcer.volume", 0.5f, CVar.ARCHIVE | CVar.CLIENTONLY);

    /// <summary>
    ///     Disables multiple announcement sounds from playing at once
    /// </summary>
    public static readonly CVarDef<bool> AnnouncerDisableMultipleSounds =
        CVarDef.Create("announcer.disable_multiple_sounds", false, CVar.ARCHIVE | CVar.CLIENTONLY);
}
