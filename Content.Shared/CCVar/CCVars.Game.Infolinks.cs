// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <igor@c4llv07e.xyz>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Link to Discord server to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksDiscord =
        CVarDef.Create("infolinks.discord", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to website to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksForum =
        CVarDef.Create("infolinks.forum", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to GitHub page to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksGithub =
        CVarDef.Create("infolinks.github", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to website to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksWebsite =
        CVarDef.Create("infolinks.website", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to wiki to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksWiki =
        CVarDef.Create("infolinks.wiki", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to Patreon. Not shown in the launcher currently.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksPatreon =
        CVarDef.Create("infolinks.patreon", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to the bug report form.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksBugReport =
        CVarDef.Create("infolinks.bug_report", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to site handling ban appeals. Shown in ban disconnect messages.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksAppeal =
        CVarDef.Create("infolinks.appeal", "", CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Link to Telegram channel to show in the launcher.
    /// </summary>
    public static readonly CVarDef<string> InfoLinksTelegram =
        CVarDef.Create("infolinks.telegram", "", CVar.SERVER | CVar.REPLICATED);
}