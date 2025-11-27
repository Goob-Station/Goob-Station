// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Configuration;

namespace Content.Shared._Funkystation.CCVar;

[CVarDefs]
public sealed class CCVarsMalfAi
{
    /// <summary>
    /// The duration (in seconds) of the Malf AI Doomsday Protocol.
    /// </summary>
    public static readonly CVarDef<float> MalfAiDoomsdayDuration =
        CVarDef.Create("malfai.doomsday_duration", 420f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// Duration in seconds that an APC remains siphoned after being targeted by a Malf AI.
    /// During this time, the APC is completely disabled and cannot supply power.
    /// </summary>
    public static readonly CVarDef<float> MalfAiSiphonDurationSeconds =
        CVarDef.Create("malfai.siphon_duration_seconds", 60f, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// The amount of CPU that a Malf AI gains per APC siphon.
    /// </summary>
    public static readonly CVarDef<int> MalfAiSiphonCpuAmount =
        CVarDef.Create("malfai.siphon_cpu_amount", 5, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// CPU cost to impose Law 0 on a borg.
    /// </summary>
    public static readonly CVarDef<int> MalfAiImposeLawCpuCost =
        CVarDef.Create("malfai.impose_law_cpu_cost", 25, CVar.SERVER | CVar.ARCHIVE);

    /// <summary>
    /// The effective range in tiles for the MalfAI camera upgrade.
    /// </summary>
    public static readonly CVarDef<float> MalfAiCameraUpgradeRange =
        CVarDef.Create("malfai.camera_upgrade_range", 6.0f, CVar.SERVER | CVar.REPLICATED);
}
