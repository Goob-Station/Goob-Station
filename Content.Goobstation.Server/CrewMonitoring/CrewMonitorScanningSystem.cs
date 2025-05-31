// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.RelayedDeathrattle;
using Content.Goobstation.Shared.CrewMonitoring;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;

namespace Content.Goobstation.Server.CrewMonitoring;

public sealed class CrewMonitorScanningSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrewMonitorScanningComponent, AfterInteractEvent>(OnScanAttempt);
        SubscribeLocalEvent<CrewMonitorScanningComponent, CrewMonitorScanningDoAfterEvent>(OnScanComplete);
    }

    private void OnScanAttempt(EntityUid uid, CrewMonitorScanningComponent comp, AfterInteractEvent args)
    {
        if (args.Target == null || !args.CanReach || !HasComp<HumanoidAppearanceComponent>(args.Target))
            return;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new CrewMonitorScanningDoAfterEvent(), uid, args.Target, uid) { NeedHand = true, BreakOnMove = true });
    }

    private void OnScanComplete(EntityUid uid, CrewMonitorScanningComponent comp, CrewMonitorScanningDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null || comp.ScannedEntities.Contains(args.Target.Value))
            return;

        if (_whitelist.IsWhitelistFail(comp.Whitelist, args.Target.Value))
            return;

        comp.ScannedEntities.Add(args.Target.Value);
        if (!comp.ApplyDeathrattle)
            return;

        EnsureComp<RelayedDeathrattleComponent>(args.Target.Value).Target = uid;
        args.Handled = true;
    }
}
