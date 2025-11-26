// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 MarkerWicker <markerWicker@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Whether height & width sliders adjust a character's Fixture Component
    /// </summary>
    public static readonly CVarDef<bool> HeightAdjustModifiesHitbox =
        CVarDef.Create("heightadjust.modifies_hitbox", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether height & width sliders adjust a player's max view distance
    /// </summary>
    public static readonly CVarDef<bool> HeightAdjustModifiesZoom =
        CVarDef.Create("heightadjust.modifies_zoom", false, CVar.SERVERONLY);

    /// <summary>
    ///     Whether height & width sliders adjust a player's bloodstream volume.
    /// </summary>
    /// <remarks>
    ///     This can be configured more precisely by modifying BloodstreamAffectedByMassComponent.
    /// </remarks>
    public static readonly CVarDef<bool> HeightAdjustModifiesBloodstream =
        CVarDef.Create("heightadjust.modifies_bloodstream", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether height & width sliders adjust a player's sprinting speed.
    /// <remarks>
    ///     This can be configured more precisely by modifying SprintingAffectedByScaleComponent.
    /// </remarks>
    public static readonly CVarDef<bool> HeightAdjustModifiesSprinting =
        CVarDef.Create("heightadjust.modifies_sprinting", true, CVar.SERVERONLY);

    /// <summary>
    ///     Whether height & width sliders adjust a harpy's flight speed and stamina drain.
    /// <remarks>
    ///     This can be configured more precisely by modifying FlightAffectedByScaleComponent.
    /// </remarks>
    public static readonly CVarDef<bool> HeightAdjustModifiesFlight =
        CVarDef.Create("heightadjust.modifies_flight", true, CVar.SERVERONLY);
}
