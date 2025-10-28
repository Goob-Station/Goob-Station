// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Zekins <zekins3366@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.Corvax.CorvaxVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
public sealed class CorvaxVars
{
    /// <summary>
    /// Responsible for turning on and off the bark system.
    /// </summary>
    public static readonly CVarDef<bool> BarksEnabled =
        CVarDef.Create("voice.barks_enabled", false, CVar.SERVER | CVar.REPLICATED | CVar.ARCHIVE);

    /// <summary>
    /// Default volume setting of Barks sound
    /// </summary>
    public static readonly CVarDef<float> BarksVolume =
        CVarDef.Create("voice.barks_volume", 1f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
