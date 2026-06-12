// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Duration of the doomsday countdown in seconds.
    /// </summary>
    public static readonly CVarDef<float> MalfAiDoomsdayDuration =
        CVarDef.Create("malfai.doomsday_duration", 420f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// How long APCs stay siphoned before restoring, in seconds.
    /// </summary>
    public static readonly CVarDef<float> MalfAiSiphonDurationSeconds =
        CVarDef.Create("malfai.siphon_duration_seconds", 60f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// How much CPU the Malf AI gains per APC siphon completion.
    /// </summary>
    public static readonly CVarDef<int> MalfAiSiphonCpuAmount =
        CVarDef.Create("malfai.siphon_cpu_amount", 5, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// CPU cost to impose a new law on a borg.
    /// </summary>
    public static readonly CVarDef<int> MalfAiImposeLawCpuCost =
        CVarDef.Create("malfai.impose_law_cpu_cost", 25, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// Range in tiles of the camera upgrade x-ray effect.
    /// </summary>
    public static readonly CVarDef<float> MalfAiCameraUpgradeRange =
        CVarDef.Create("malfai.camera_upgrade_range", 6.0f, CVar.SERVER | CVar.REPLICATED);
}
