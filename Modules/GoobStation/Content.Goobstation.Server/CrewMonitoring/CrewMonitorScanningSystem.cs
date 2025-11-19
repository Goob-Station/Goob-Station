// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.RelayedDeathrattle;
using Content.Goobstation.Shared.CrewMonitoring;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Shared.Interaction;
using Content.Shared.Whitelist;
using Content.Shared.Implants;
using Content.Shared.IdentityManagement;
using Content.Server.Popups;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.CrewMonitoring;

public sealed class CrewMonitorScanningSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    [Dependency] private readonly SharedSubdermalImplantSystem _implantSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    private const string CommandTrackerImplant = "CommandTrackingImplant";
    private const string CommandTrackerImplantName = "command tracking implant";

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

        var userName = Identity.Entity(args.User, EntityManager);
        _popup.PopupEntity(Loc.GetString("injector-component-injecting-user"), args.Target.Value, args.User);
        if (args.User != args.Target.Value)
            _popup.PopupEntity(Loc.GetString("implanter-component-implanting-target", ("user", userName)), args.User, args.Target.Value, PopupType.LargeCaution);

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new CrewMonitorScanningDoAfterEvent(), uid, args.Target, uid) { NeedHand = true, BreakOnMove = true });
    }

    private void OnScanComplete(EntityUid uid, CrewMonitorScanningComponent comp, CrewMonitorScanningDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Target == null)
            return;
        var name = Identity.Name(args.Target.Value, EntityManager, args.User);

        if (comp.ScannedEntities.Contains(args.Target.Value))
        {
            var msg = Loc.GetString("implanter-component-implant-already", ("implant", CommandTrackerImplantName), ("target", name));
            _popup.PopupEntity(msg, args.Target.Value, args.User);
            return;
        }

        if (_whitelist.IsWhitelistFail(comp.Whitelist, args.Target.Value))
        {

            var msg = Loc.GetString("implanter-component-implant-failed", ("implant", CommandTrackerImplantName), ("target", name));
            _popup.PopupEntity(msg, args.Target.Value, args.User);
            return;
        }

        comp.ScannedEntities.Add(args.Target.Value); //Keep for don't double implant
        _implantSystem.AddImplant(args.Target.Value, CommandTrackerImplant);

        if (comp.ApplyDeathrattle)
            EnsureComp<RelayedDeathrattleComponent>(args.Target.Value).Target = uid;

        args.Handled = true;
    }
}
