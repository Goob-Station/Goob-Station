using System;
using System.Collections.Generic;
using Content.Shared.MalfAI.Actions;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Electrocution;
using Robust.Shared.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Audio;
using Content.Server.Chat.Systems;
using Robust.Shared.Localization;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Server.MalfAI;

/// <summary>
/// Handles the Malf AI Station Lockdown action. Extracted from MalfAiShopSystem.
/// </summary>
public sealed class MalfAiLockdownSystem : EntitySystem
{
    [Dependency] private readonly SharedDoorSystem _doors = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrify = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedAirlockSystem _airlocks = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Content.Shared.Store.Components.StoreComponent, MalfAiLockdownGridActionEvent>(OnLockdownGridAction);
    }

    private void OnLockdownGridAction(EntityUid uid, Content.Shared.Store.Components.StoreComponent comp, ref MalfAiLockdownGridActionEvent args)
    {
        var performer = args.Performer != default ? args.Performer : uid;
        var duration = TimeSpan.FromSeconds(args.Duration);

        var announcement = Loc.GetString("malfai-lockdown-announcement");
        _chat.DispatchStationAnnouncement(
            performer,
            announcement,
            sender: Loc.GetString("malfai-lockdown-sender"),
            playDefaultSound: true,
            announcementSound: new SoundPathSpecifier("/Audio/Misc/notice1.ogg"),
            colorOverride: Color.Red);

        LockdownGrid(performer, duration);
        args.Handled = true;
    }

    private void LockdownGrid(EntityUid origin, TimeSpan duration)
    {
        if (!Exists(origin))
            return;

        var xform = Transform(origin);
        var gridUid = xform.GridUid;
        if (gridUid == null)
            return;

        var affected = new List<EntityUid>();
        var bolted = new List<EntityUid>();
        var electrified = new List<EntityUid>();
        var safetyDisabled = new List<EntityUid>();
        var query = EntityQueryEnumerator<DoorComponent, TransformComponent>();
        while (query.MoveNext(out var doorUid, out var door, out var dXform))
        {
            if (dXform.GridUid != gridUid)
                continue;

            affected.Add(doorUid);

            if (TryComp<ElectrifiedComponent>(doorUid, out var electrifiedComp) && !electrifiedComp.Enabled)
            {
                _electrify.SetElectrified((doorUid, electrifiedComp), true);
                electrified.Add(doorUid);
            }

            if (TryComp<AirlockComponent>(doorUid, out var airlock) && airlock.Safety)
            {
                _airlocks.SetSafety(airlock, false);
                safetyDisabled.Add(doorUid);
            }

            var isBoltable = TryComp<DoorBoltComponent>(doorUid, out var boltComp);

            if (door.State == DoorState.Closed && isBoltable)
            {
                _doors.SetBoltsDown((doorUid, boltComp!), true);
                bolted.Add(doorUid);
                continue;
            }

            _doors.TryClose(doorUid, door, null);

            if (isBoltable)
            {
                var boltDelay = door.CloseTimeOne + door.CloseTimeTwo + TimeSpan.FromMilliseconds(50);
                var target = doorUid;
                Timer.Spawn(boltDelay, () =>
                {
                    if (!Exists(target))
                        return;

                    if (!TryComp<DoorBoltComponent>(target, out var currentBolts))
                        return;

                    _doors.SetBoltsDown((target, currentBolts), true);
                    bolted.Add(target);
                });
            }
        }

        Timer.Spawn(duration, () =>
        {
            foreach (var doorUid in electrified)
            {
                if (!Exists(doorUid))
                    continue;

                if (TryComp<ElectrifiedComponent>(doorUid, out var ecomp) && ecomp.Enabled)
                {
                    _electrify.SetElectrified((doorUid, ecomp), false);
                }
            }

            foreach (var doorUid in bolted)
            {
                if (!Exists(doorUid))
                    continue;

                if (TryComp<DoorBoltComponent>(doorUid, out var bolts))
                {
                    _doors.SetBoltsDown((doorUid, bolts), false);
                }
            }

            foreach (var doorUid in safetyDisabled)
            {
                if (!Exists(doorUid))
                    continue;

                if (TryComp<AirlockComponent>(doorUid, out var airlock) && !airlock.Safety)
                {
                    _airlocks.SetSafety(airlock, true);
                }
            }

            foreach (var doorUid in affected)
            {
                if (!Exists(doorUid))
                    continue;

                _doors.TryOpen(doorUid);
            }
        });
    }
}
