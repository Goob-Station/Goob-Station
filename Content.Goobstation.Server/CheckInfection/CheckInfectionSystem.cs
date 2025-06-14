// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.CheckInfection;
using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Robust.Server.Audio;

namespace Content.Goobstation.Server.CheckInfection;

public sealed partial class CheckInfectionSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doafter = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CheckInfectionComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<CheckInfectionComponent, CheckInfectionDoAfter>(OnDoAfter);
        SubscribeLocalEvent<CheckInfectionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnAfterInteract(EntityUid uid, CheckInfectionComponent component, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is null)
            return;

        var doAfterArgs = new DoAfterArgs(EntityManager,
            args.User,
            component.DoAfterDuration,
            new CheckInfectionDoAfter(),
            uid,
            args.Target,
            args.Used)
        {
            BreakOnMove = true,
            NeedHand = true,
            BlockDuplicate = true,
            BreakOnHandChange = true,
        };

        _doafter.TryStartDoAfter(doAfterArgs);

        var popup = Loc.GetString("check-infection-start", ("user", args.User), ("target", args.Target.Value));
        _popup.PopupEntity(popup, uid, PopupType.SmallCaution);
    }

    private void OnDoAfter(EntityUid uid, CheckInfectionComponent component, ref CheckInfectionDoAfter args)
    {
        if (args.Cancelled
        || args.Handled
        || args.Target is not { } target)
        return;

        _audio.PlayPvs(component.ScanningEndSound, uid);
        component.LastTarget = target;

        if (!TryComp<PendingZombieComponent>(target, out var zed))
        {
            var clearString = Loc.GetString("check-infection-clear");
            _popup.PopupEntity(clearString, uid, PopupType.Medium);
            component.WasInfected = false;

            return;
        }

        var infectedString = Loc.GetString("check-infection-infected", ("time", (int)zed.GracePeriod.TotalSeconds));
        _popup.PopupEntity(infectedString, uid, PopupType.MediumCaution);
        component.WasInfected = true;

    }

    private void OnExamined(EntityUid uid, CheckInfectionComponent component, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
        || component.LastTarget is not { } lastTarget)
            return;

        var target = Loc.GetString("check-infection-examined-target", ("target", lastTarget));
        var infectionStatus = Loc.GetString("check-infection-examined-infection-status", ("status", component.WasInfected));

        args.PushMarkup(target, 1);
        args.PushMarkup(infectionStatus);
    }

}
