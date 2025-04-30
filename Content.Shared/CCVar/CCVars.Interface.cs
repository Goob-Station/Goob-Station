// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// The sound played when clicking a UI button
    /// </summary>
    public static readonly CVarDef<string> UIClickSound =
        CVarDef.Create("interface.click_sound", "/Audio/UserInterface/click.ogg", CVar.REPLICATED);

    /// <summary>
    /// The sound played when the mouse hovers over a clickable UI element
    /// </summary>
    public static readonly CVarDef<string> UIHoverSound =
        CVarDef.Create("interface.hover_sound", "/Audio/UserInterface/hover.ogg", CVar.REPLICATED);

    /// <summary>
    /// The layout style of the UI
    /// </summary>
    public static readonly CVarDef<string> UILayout =
        CVarDef.Create("ui.layout", "Default", CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// The dimensions for the chat window in Default UI mode
    /// </summary>
    public static readonly CVarDef<string> DefaultScreenChatSize =
        CVarDef.Create("ui.default_chat_size", "", CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// The width of the chat panel in Separated UI mode
    /// </summary>
    public static readonly CVarDef<string> SeparatedScreenChatSize =
        CVarDef.Create("ui.separated_chat_size", "0.6,0", CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> OutlineEnabled =
        CVarDef.Create("outline.enabled", true, CVar.CLIENTONLY);

    /// <summary>
    /// If true, the admin overlay will be displayed in the old style (showing only "ANTAG")
    /// </summary>
    public static readonly CVarDef<bool> AdminOverlayClassic =
        CVarDef.Create("ui.admin_overlay_classic", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// If true, the admin overlay will display the total time of the players
    /// </summary>
    public static readonly CVarDef<bool> AdminOverlayPlaytime =
        CVarDef.Create("ui.admin_overlay_playtime", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// If true, the admin overlay will display the players starting position.
    /// </summary>
    public static readonly CVarDef<bool> AdminOverlayStartingJob =
        CVarDef.Create("ui.admin_overlay_starting_job", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
