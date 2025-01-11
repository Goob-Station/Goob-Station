using Robust.Shared.Configuration;

namespace Content.Shared._Goobstation.CCVar;

[CVarDefs]
public sealed partial class GoobCVars
{
    /// <summary>
    ///     Whether pipes will unanchor on ANY conflicting connection. May break maps.
    ///     If false, allows you to stack pipes as long as new directions are added (i.e. in a new pipe rotation, layer or multi-Z link), otherwise unanchoring them.
    /// </summary>
    public static readonly CVarDef<bool> StrictPipeStacking =
        CVarDef.Create("atmos.strict_pipe_stacking", false, CVar.SERVERONLY);

    /// <summary>
    ///     If an object's mass is below this number, then this number is used in place of mass to determine whether air pressure can throw an object.
    ///     This has nothing to do with throwing force, only acting as a way of reducing the odds of tiny 5 gram objects from being yeeted by people's breath
    /// </summary>
    /// <remarks>
    ///     If you are reading this because you want to change it, consider looking into why almost every item in the game weighs only 5 grams
    ///     And maybe do your part to fix that? :)
    /// </remarks>
    public static readonly CVarDef<float> SpaceWindMinimumCalculatedMass =
        CVarDef.Create("atmos.space_wind_minimum_calculated_mass", 10f, CVar.SERVERONLY);

    /// <summary>
    /// 	Calculated as 1/Mass, where Mass is the physics.Mass of the desired threshold.
    /// 	If an object's inverse mass is lower than this, it is capped at this. Basically, an upper limit to how heavy an object can be before it stops resisting space wind more.
    /// </summary>
    public static readonly CVarDef<float> SpaceWindMaximumCalculatedInverseMass =
        CVarDef.Create("atmos.space_wind_maximum_calculated_inverse_mass", 0.04f, CVar.SERVERONLY);

    /// <summary>
    /// Increases default airflow calculations to O(n^2) complexity, for use with heavy space wind optimizations. Potato servers BEWARE
    /// This solves the problem of objects being trapped in an infinite loop of slamming into a wall repeatedly.
    /// </summary>
    public static readonly CVarDef<bool> MonstermosUseExpensiveAirflow =
        CVarDef.Create("atmos.mmos_expensive_airflow", true, CVar.SERVERONLY);

    /// <summary>
    ///     A multiplier on the amount of force applied to Humanoid entities, as tracked by HumanoidAppearanceComponent
    ///     This multiplier is added after all other checks are made, and applies to both throwing force, and how easy it is for an entity to be thrown.
    /// </summary>
    public static readonly CVarDef<float> AtmosHumanoidThrowMultiplier =
        CVarDef.Create("atmos.humanoid_throw_multiplier", 2f, CVar.SERVERONLY);

    /// <summary>
    ///     Taken as the cube of a tile's mass, this acts as a minimum threshold of mass for which air pressure calculates whether or not to rip a tile from the floor
    ///     This should be set by default to the cube of the game's lowest mass tile as defined in their prototypes, but can be increased for server performance reasons
    /// </summary>
    public static readonly CVarDef<float> MonstermosRipTilesMinimumPressure =
        CVarDef.Create("atmos.monstermos_rip_tiles_min_pressure", 7500f, CVar.SERVERONLY);

    /// <summary>
    ///     Taken after the minimum pressure is checked, the effective pressure is multiplied by this amount.
    ///		This allows server hosts to finely tune how likely floor tiles are to be ripped apart by air pressure
    /// </summary>
    public static readonly CVarDef<float> MonstermosRipTilesPressureOffset =
        CVarDef.Create("atmos.monstermos_rip_tiles_pressure_offset", 0.44f, CVar.SERVERONLY);

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
        CVarDef.Create("goob.silo_enabled", true, CVar.SERVER | CVar.REPLICATED);

    #region Player Listener

    /// <summary>
    ///     Enable Dorm Notifier
    /// </summary>
    public static readonly CVarDef<bool> DormNotifier =
        CVarDef.Create("dorm_notifier.enable", true, CVar.SERVER);

    /// <summary>
    ///     Check for dorm activity every X amount of ticks
    ///     Default is 10.
    /// </summary>
    public static readonly CVarDef<int> DormNotifierFrequency =
        CVarDef.Create("dorm_notifier.frequency", 10, CVar.SERVER);

    /// <summary>
    ///     Time given to be found to be engaging in dorm activity
    ///     Default is 120.
    /// </summary>
    public static readonly CVarDef<int> DormNotifierPresenceTimeout =
        CVarDef.Create("dorm_notifier.timeout", 120, CVar.SERVER, "Mark as condemned if present near a dorm marker for more than X amount of seconds.");

    /// <summary>
    ///     Time given to be found engaging in dorm activity if any of the sinners are nude
    ///     Default if 25.
    /// </summary>
    public static readonly CVarDef<int> DormNotifierPresenceTimeoutNude =
        CVarDef.Create("dorm_notifier.timeout_nude", 25, CVar.SERVER, "Mark as condemned if present near a dorm marker for more than X amount of seconds while being nude.");

    /// <summary>
    ///     Broadcast to all players that a player has ragequit.
    /// </summary>
    public static readonly CVarDef<bool> PlayerRageQuitNotify =
        CVarDef.Create("ragequit.notify", true, CVar.SERVERONLY);

    /// <summary>
    ///     Time between being eligible for a "rage quit" after reaching a damage threshold.
    ///     Default is 5f.
    /// </summary>
    public static readonly CVarDef<float> PlayerRageQuitTimeThreshold =
        CVarDef.Create("ragequit.threshold", 30f, CVar.SERVERONLY);

    /// <summary>
    ///     Log ragequits to a discord webhook, set to empty to disable.
    /// </summary>
    public static readonly CVarDef<string> PlayerRageQuitDiscordWebhook =
        CVarDef.Create("ragequit.discord_webhook", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    #endregion PlayerListener

    #region Surgery

    public static readonly CVarDef<bool> CanOperateOnSelf =
        CVarDef.Create("surgery.can_operate_on_self", true, CVar.SERVERONLY);

    #endregion

    #region Discord AHelp Reply System

    /// <summary>
    ///     If an admin replies to users from discord, should it use their discord role color? (if applicable)
    ///     Overrides DiscordReplyColor and AdminBwoinkColor.
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleColor =
        CVarDef.Create("admin.use_discord_role_color", true, CVar.SERVERONLY);

    /// <summary>
    ///     If an admin replies to users from discord, should it use their discord role name? (if applicable)
    /// </summary>
    public static readonly CVarDef<bool> UseDiscordRoleName =
        CVarDef.Create("admin.use_discord_role_name", true, CVar.SERVERONLY);

    /// <summary>
    ///     The text before an admin's name when replying from discord to indicate they're speaking from discord.
    /// </summary>
    public static readonly CVarDef<string> DiscordReplyPrefix =
        CVarDef.Create("admin.discord_reply_prefix", "(DISCORD) ", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of admins. This is the fallback color for admins.
    /// </summary>
    public static readonly CVarDef<string> AdminBwoinkColor =
        CVarDef.Create("admin.admin_bwoink_color", "red", CVar.SERVERONLY);

    /// <summary>
    ///     The color of the names of admins who reply from discord. Leave empty to disable.
    ///     Overrides AdminBwoinkColor.
    /// </summary>
    public static readonly CVarDef<string> DiscordReplyColor =
        CVarDef.Create("admin.discord_reply_color", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Use the admin's Admin OOC color in bwoinks.
    ///     If either the ooc color or this is not set, uses the admin.admin_bwoink_color value.
    /// </summary>
    public static readonly CVarDef<bool> UseAdminOOCColorInBwoinks =
        CVarDef.Create("admin.bwoink_use_admin_ooc_color", true, CVar.SERVERONLY);

    #endregion

    /// <summary>
    ///     Goobstation: The amount of time between NPC Silicons draining their battery in seconds.
    /// </summary>
    public static readonly CVarDef<float> SiliconNpcUpdateTime =
        CVarDef.Create("silicon.npcupdatetime", 1.5f, CVar.SERVERONLY);

    /// <summary>
    ///     Should the player automatically get up after being knocked down
    /// </summary>
    public static readonly CVarDef<bool> AutoGetUp =
        CVarDef.Create("white.auto_get_up", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED); // WD EDIT

    #region Blob
    public static readonly CVarDef<int> BlobMax =
        CVarDef.Create("blob.max", 3, CVar.SERVERONLY);

    public static readonly CVarDef<int> BlobPlayersPer =
        CVarDef.Create("blob.players_per", 20, CVar.SERVERONLY);

    public static readonly CVarDef<bool> BlobCanGrowInSpace =
        CVarDef.Create("blob.grow_space", true, CVar.SERVER);

    #endregion

    #region Mechs

    /// <summary>
    ///     Whether or not players can use mech guns outside of mechs.
    /// </summary>
    public static readonly CVarDef<bool> MechGunOutsideMech =
        CVarDef.Create("mech.gun_outside_mech", true, CVar.SERVER | CVar.REPLICATED);

    #endregion
}
