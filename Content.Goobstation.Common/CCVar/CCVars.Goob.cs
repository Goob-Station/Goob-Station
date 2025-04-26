// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2025 Ducks <97200673+TwoDucksOnnaPlane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Eagle <lincoln.mcqueen@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ichaie <167008606+Ichaie@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 JORJ949 <159719201+JORJ949@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 MortalBaguette <169563638+MortalBaguette@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Panela <107573283+AgentePanela@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Poips <Hanakohashbrown@gmail.com>
// SPDX-FileCopyrightText: 2025 PuroSlavKing <103608145+PuroSlavKing@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 Whisper <121047731+QuietlyWhisper@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 blobadoodle <me@bloba.dev>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 kamkoi <poiiiple1@gmail.com>
// SPDX-FileCopyrightText: 2025 shibe <95730644+shibechef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 tetra <169831122+Foralemes@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 vanx <61917534+Vaaankas@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

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

    /// <summary>
    ///     Set a max drunk time in seconds to prevent permanent drunkeness. 
    /// </summary>
    public static readonly CVarDef<float> MaxDrunkTime =
        CVarDef.Create("goob.max_drunk_time", 1500f, CVar.SERVER | CVar.REPLICATED);

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
    ///     Default is 180.
    /// </summary>
    public static readonly CVarDef<int> DormNotifierPresenceTimeout =
        CVarDef.Create("dorm_notifier.timeout", 180, CVar.SERVER, "Mark as condemned if present near a dorm marker for more than X amount of seconds.");

    /// <summary>
    ///     Time given to be found engaging in dorm activity if any of the sinners are nude
    ///     Default if 60.
    /// </summary>
    public static readonly CVarDef<int> DormNotifierPresenceTimeoutNude =
        CVarDef.Create("dorm_notifier.timeout_nude", 60, CVar.SERVER, "Mark as condemned if present near a dorm marker for more than X amount of seconds while being nude.");

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

    #region RMC

    public static readonly CVarDef<int> RMCPatronLobbyMessageTimeSeconds =
        CVarDef.Create("rmc.patron_lobby_message_time_seconds", 30, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<int> RMCPatronLobbyMessageInitialDelaySeconds =
        CVarDef.Create("rmc.patron_lobby_message_initial_delay_seconds", 5, CVar.REPLICATED | CVar.SERVER);

    public static readonly CVarDef<string> RMCDiscordAccountLinkingMessageLink =
        CVarDef.Create("rmc.discord_account_linking_message_link", "", CVar.REPLICATED | CVar.SERVER);

    #endregion

    #region Goobcoins

    public static readonly CVarDef<int> GoobcoinsPerPlayer =
        CVarDef.Create("goob.coins_per_player", 10, CVar.SERVERONLY);

    public static readonly CVarDef<int> GoobcoinsPerGreentext =
        CVarDef.Create("goob.coins_per_greentext", 5, CVar.SERVERONLY);

    public static readonly CVarDef<int> GoobcoinNonAntagMultiplier =
        CVarDef.Create("goob.coins_non_antag_multiplier", 1, CVar.SERVERONLY);

    public static readonly CVarDef<int> GoobcoinServerMultiplier =
        CVarDef.Create("goob.coins_server_multiplier", 1, CVar.SERVERONLY);

    public static readonly CVarDef<int> GoobcoinMinPlayers =
        CVarDef.Create("goob.coins_min_players", 5, CVar.SERVERONLY);

    #endregion

    #region Game Director

    public static readonly CVarDef<float> MinimumTimeUntilFirstEvent =
        CVarDef.Create("gamedirector.minimumtimeuntilfirstevent", 300f, CVar.SERVERONLY);

    public static readonly CVarDef<int> GameDirectorDebugPlayerCount =
        CVarDef.Create("gamedirector.debug_player_count", 80, CVar.SERVERONLY);

    #endregion

    #region Contests System

    /// <summary>
    ///     The MASTER TOGGLE for the entire Contests System.
    ///     ALL CONTESTS BELOW, regardless of type or setting will output 1f when false.
    /// </summary>
    public static readonly CVarDef<bool> DoContestsSystem =
        CVarDef.Create("contests.do_contests_system", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Contest functions normally include an optional override to bypass the clamp set by max_percentage.
    ///     This CVar disables the bypass when false, forcing all implementations to comply with max_percentage.
    /// </summary>
    public static readonly CVarDef<bool> AllowClampOverride =
        CVarDef.Create("contests.allow_clamp_override", true, CVar.REPLICATED | CVar.SERVER);
    /// <summary>
    ///     Toggles all MassContest functions. All mass contests output 1f when false
    /// </summary>
    public static readonly CVarDef<bool> DoMassContests =
        CVarDef.Create("contests.do_mass_contests", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Toggles all StaminaContest functions. All stamina contests output 1f when false
    /// </summary>
    public static readonly CVarDef<bool> DoStaminaContests =
        CVarDef.Create("contests.do_stamina_contests", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Toggles all HealthContest functions. All health contests output 1f when false
    /// </summary>
    public static readonly CVarDef<bool> DoHealthContests =
        CVarDef.Create("contests.do_health_contests", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Toggles all MindContest functions. All mind contests output 1f when false.
    ///     MindContests are not currently implemented, and are awaiting completion of the Psionic Refactor
    /// </summary>
    public static readonly CVarDef<bool> DoMindContests =
        CVarDef.Create("contests.do_mind_contests", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     The maximum amount that Mass Contests can modify a physics multiplier, given as a +/- percentage
    ///     Default of 0.25f outputs between * 0.75f and 1.25f
    /// </summary>
    public static readonly CVarDef<float> MassContestsMaxPercentage =
        CVarDef.Create("contests.max_percentage", 1f, CVar.REPLICATED | CVar.SERVER);

    #endregion

    #region Shoving - WD Port
    /// <summary>
    /// Shove range multiplier.
    /// </summary>
    public static readonly CVarDef<float> ShoveRange =
        CVarDef.Create("game.shove_range", 3f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// Shove speed multiplier, does not affect range.
    /// </summary>
    public static readonly CVarDef<float> ShoveSpeed =
        CVarDef.Create("game.shove_speed", 4f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// How much should the mass difference affect shove range & speed.
    /// </summary>
    public static readonly CVarDef<float> ShoveMassFactor =
        CVarDef.Create("game.shove_mass_factor", 5f, CVar.SERVER | CVar.ARCHIVE);
    #endregion

    #region Chat

    /// <summary>
    /// A string containing a list of newline-separated words to be highlighted in the chat.
    /// </summary>
    public static readonly CVarDef<string> ChatHighlights =
        CVarDef.Create("chat.highlights", "", CVar.CLIENTONLY | CVar.ARCHIVE, "A list of newline-separated words to be highlighted in the chat.");

    /// <summary>
    /// An option to toggle the automatic filling of the highlights with the character's info, if available.
    /// </summary>
    public static readonly CVarDef<bool> ChatAutoFillHighlights =
        CVarDef.Create("chat.auto_fill_highlights", false, CVar.CLIENTONLY | CVar.ARCHIVE, "Toggles automatically filling the highlights with the character's information.");

    /// <summary>
    /// The color in which the highlights will be displayed.
    /// </summary>
    public static readonly CVarDef<string> ChatHighlightsColor =
        CVarDef.Create("chat.highlights_color", "#17FFC1FF", CVar.CLIENTONLY | CVar.ARCHIVE, "The color in which the highlights will be displayed.");

    /// <summary>
    /// Whether or not to log actions in the chat.
    /// </summary>
    public static readonly CVarDef<bool> LogInChat =
        CVarDef.Create("chat.log_in_chat", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    /// <summary>
    /// Whether or not to coalesce identical messages in the chat.
    /// </summary>
    public static readonly CVarDef<bool> CoalesceIdenticalMessages =
         CVarDef.Create("chat.coalesce_identical_messages", true, CVar.CLIENT | CVar.ARCHIVE | CVar.CLIENTONLY);

    #endregion

    #region Misc

    /// <summary>
    /// Whether or not to show detailed examine text.
    /// </summary>
    public static readonly CVarDef<bool> DetailedExamine =
        CVarDef.Create("misc.detailed_examine", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    #endregion
}