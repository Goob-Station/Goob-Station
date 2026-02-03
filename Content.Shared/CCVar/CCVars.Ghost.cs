// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The time you must spend reading the rules, before the "Request" button is enabled
    /// </summary>
    public static readonly CVarDef<float> GhostRoleTime =
        CVarDef.Create("ghost.role_time", 3f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     If ghost role lotteries should be made near-instanteous.
    /// </summary>
    public static readonly CVarDef<bool> GhostQuickLottery =
        CVarDef.Create("ghost.quick_lottery", false, CVar.SERVERONLY);

    /// <summary>
    ///     Whether or not to kill the player's mob on ghosting, when it is in a critical health state.
    /// </summary>
    public static readonly CVarDef<bool> GhostKillCrit =
        CVarDef.Create("ghost.kill_crit", true, CVar.REPLICATED | CVar.SERVER);

    #region DOWNSTREAM-TPirates: ghost follow menu update
    /// <summary>
    ///     Maximum number of mob (NPC) warps to include in the ghost follow/orbit menu. Limits server load when many mobs exist.
    /// </summary>
    public static readonly CVarDef<int> GhostWarpMaxMobs =
        CVarDef.Create("ghost.warp_max_mobs", 150, CVar.SERVER);

    /// <summary>
    ///     Maximum number of dead (corpse) warps to include in the ghost follow/orbit menu. Limits server load when many dead entities exist.
    /// </summary>
    public static readonly CVarDef<int> GhostWarpMaxDead =
        CVarDef.Create("ghost.warp_max_dead", 100, CVar.SERVER);
    #endregion
}