// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /*
     * Server
     */

    /// <summary>
    ///     Change this to have the changelog and rules "last seen" date stored separately.
    /// </summary>
    public static readonly CVarDef<string> ServerId =
        CVarDef.Create("server.id", "Goobstation", CVar.REPLICATED | CVar.SERVER); // if ur a fork of goob pls change this thanks :)

    /// <summary>
    ///     Guide Entry Prototype ID to be displayed as the server rules.
    /// </summary>
    public static readonly CVarDef<string> RulesFile =
        CVarDef.Create("server.rules_file", "DefaultRuleset", CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Guide entry that is displayed by default when a guide is opened.
    /// </summary>
    public static readonly CVarDef<string> DefaultGuide =
        CVarDef.Create("server.default_guide", "NewPlayer", CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     If greater than 0, automatically restart the server after this many minutes of uptime.
    /// </summary>
    /// <remarks>
    /// <para>
    ///     This is intended to work around various bugs and performance issues caused by long continuous server uptime.
    /// </para>
    /// <para>
    ///     This uses the same non-disruptive logic as update restarts,
    ///     i.e. the game will only restart at round end or when there is nobody connected.
    /// </para>
    /// </remarks>
    public static readonly CVarDef<int> ServerUptimeRestartMinutes =
        CVarDef.Create("server.uptime_restart_minutes", 0, CVar.SERVERONLY);

    /// <summary>
    ///     This will be the title shown in the lobby
    ///     If empty, the title will be {ui-lobby-title} + the server's full name from the hub
    /// </summary>
    public static readonly CVarDef<string> ServerLobbyName =
        CVarDef.Create("server.lobby_name", "", CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     The width of the right side (chat) panel in the lobby
    /// </summary>
    public static readonly CVarDef<int> ServerLobbyRightPanelWidth =
        CVarDef.Create("server.lobby_right_panel_width", 650, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Forces clients to display version watermark, as if HudVersionWatermark was true
    /// </summary>
    public static readonly CVarDef<bool> ForceClientHudVersionWatermark =
        CVarDef.Create("server.force_client_hud_version_watermark", false, CVar.REPLICATED | CVar.SERVER);
}