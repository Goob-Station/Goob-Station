// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Goobstation.Common.CCVar;

public sealed partial class GoobCVars
{
    /// <summary>Maximum custom ghost sprite side in pixels; 0 disables scaling.</summary>
    public static readonly CVarDef<int> CustomGhostMaxSize =
        CVarDef.Create("oskarrr.custom_ghost_max_size", 32, CVar.SERVER | CVar.REPLICATED);
}
