// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Administration.Logs;
using Content.Server.Power.Components;
using Content.Server.Store.Systems;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared.Store.Components;
using Robust.Shared.Configuration;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI's APC siphoning.
/// Pretty much their only method of getting CPU.
/// The siphon lifecycle (disable/restore APC) is handled by ApcSystem;
/// this system only grants the CPU reward.
/// </summary>
public sealed class MalfAiApcSiphonSystem : EntitySystem
{
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private const string CpuCurrency = "CPU";

    public void OnApcStartSiphon(EntityUid uid, ApcComponent apc, ref ApcStartSiphonEvent args)
    {
        if (!TryComp<StoreComponent>(args.SiphonedBy, out var store))
            return;

        var cpuAmount = _cfg.GetCVar(CCVars.MalfAiSiphonCpuAmount);
        var siphonAmount = FixedPoint2.New(cpuAmount);

        // Grant CPU to the AI
        var dict = new Dictionary<string, FixedPoint2> { { CpuCurrency, siphonAmount } };
        _store.TryAddCurrency(dict, args.SiphonedBy, store);

        // Log the APC siphoning for admin records
        _adminLogger.Add(LogType.Action, LogImpact.High, $"Malf AI {ToPrettyString(args.SiphonedBy)} siphoned APC {ToPrettyString(uid)} for {cpuAmount} CPU");
    }
}
