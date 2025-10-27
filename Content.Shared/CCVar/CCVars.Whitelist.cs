// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Controls whether the server will deny any players that are not whitelisted in the DB.
    /// </summary>
    public static readonly CVarDef<bool> WhitelistEnabled =
        CVarDef.Create("whitelist.enabled", false, CVar.SERVERONLY);

    /// <summary>
    ///     Specifies the whitelist prototypes to be used by the server. This should be a comma-separated list of prototypes.
    ///     If a whitelists conditions to be active fail (for example player count), the next whitelist will be used instead. If no whitelist is valid, the player will be allowed to connect.
    /// </summary>
    public static readonly CVarDef<string> WhitelistPrototypeList =
        CVarDef.Create("whitelist.prototype_list", "basicWhitelist", CVar.SERVERONLY);
}