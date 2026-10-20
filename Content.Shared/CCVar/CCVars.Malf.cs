// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     How much CPU the Malf AI receives from siphoning an APC.
    /// </summary>
    [CVarControl(AdminFlags.VarEdit)]
    public static readonly CVarDef<float>
        MalfAiCpuSiphonAmount = CVarDef.Create("malf.cpu_siphon_amount", 15f, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     How long (in seconds) an APC remains siphoned before the Malf AI can siphon it again.
    /// </summary>
    [CVarControl(AdminFlags.VarEdit)]
    public static readonly CVarDef<float>
        MalfAiCpuSiphonDuration = CVarDef.Create("malf.cpu_siphon_duration", 180f, CVar.SERVERONLY | CVar.ARCHIVE);

    /// <summary>
    ///     Countdown duration (in seconds) for the Malf AI Doomsday Protocol.
    /// </summary>
    [CVarControl(AdminFlags.VarEdit)]
    public static readonly CVarDef<float>
        MalfAiDoomsdayCountdown = CVarDef.Create("malf.doomsday_countdown", 300f, CVar.SERVERONLY | CVar.ARCHIVE);

    public static CVarDef<float> MalfAiApcSiphonCpu => MalfAiCpuSiphonAmount;
    public static CVarDef<float> MalfAiApcSiphonCooldown => MalfAiCpuSiphonDuration;
}
