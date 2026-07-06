// SPDX-FileCopyrightText: 2026 Jonikibaka
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Light.Components;
using Content.Server.Mind;
using Content.Server.Pinpointer;
using Content.Server.Power.Components;
using Content.Shared.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Server.Silicons.Laws;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared.SurveillanceCamera.Components;
using Content.Server.VoiceMask;
using Content.Goobstation.Shared.MalfunctionAi;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Chat.RadioIconsEvents;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.VoiceMask;
using Robust.Shared.Player;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Electrocution;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Shared.Turrets;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Popups;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Verbs;
using System.Numerics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Server.MalfunctionAi;

public sealed partial class MalfunctionAiSystem
{

    // --- Lockdown ---

    private void OnLockdown(Entity<MalfunctionAiComponent> ent, ref MalfLockdownEvent args)
    {
        if (args.Handled || !TryComp<MalfLockdownComponent>(ent.Owner, out var lockdown))
            return;

        if (lockdown.EndTime != null)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-lockdown-active"), ent.Owner);
            return;
        }

        var gridUid = Transform(ent.Owner).GridUid;
        lockdown.LockedDoors.Clear();

        var query = EntityQueryEnumerator<DoorBoltComponent, TransformComponent>();
        while (query.MoveNext(out var doorUid, out var bolt, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!_doors.TrySetBoltDown((doorUid, bolt), true, ent.Owner, predicted: false))
                continue;

            if (TryComp<ElectrifiedComponent>(doorUid, out var electrified))
                _electrify.SetElectrified((doorUid, electrified), true);

            lockdown.LockedDoors.Add(doorUid);
        }

        lockdown.EndTime = _timing.CurTime + TimeSpan.FromSeconds(lockdown.Duration);

        AnnounceFromAi(ent.Owner, Loc.GetString("malfunction-ai-announcement-lockdown"));

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-lockdown-success", ("count", lockdown.LockedDoors.Count)), ent.Owner);
        args.Handled = true;
    }

    private void EndLockdown(Entity<MalfLockdownComponent> ent)
    {
        foreach (var doorUid in ent.Comp.LockedDoors)
        {
            if (TryComp<DoorBoltComponent>(doorUid, out var bolt))
                _doors.TrySetBoltDown((doorUid, bolt), false, ent.Owner, predicted: false);

            if (TryComp<ElectrifiedComponent>(doorUid, out var electrified))
                _electrify.SetElectrified((doorUid, electrified), false);
        }

        ent.Comp.LockedDoors.Clear();
        ent.Comp.EndTime = null;
    }

    // --- Doomsday ---

    private void OnDoomsday(Entity<MalfunctionAiComponent> ent, ref MalfDoomsdayEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.DoomsdayUsed)
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-doomsday-already-used"), ent.Owner);
            return;
        }

        // The device can be bought early, but arming it requires enough hacked APCs.
        if (ent.Comp.HackedApcCount < ent.Comp.DoomsdayRequiredApcs)
        {
            _popups.PopupCursor(
                Loc.GetString("malfunction-ai-popup-doomsday-need-apcs",
                    ("required", ent.Comp.DoomsdayRequiredApcs),
                    ("current", ent.Comp.HackedApcCount)),
                ent.Owner);
            return;
        }

        ent.Comp.DoomsdayUsed = true;
        Dirty(ent);

        var doomEv = new MalfDoomsdayArmedEvent(ent.Owner);
        RaiseLocalEvent(ref doomEv);
        args.Handled = true;
    }

    // --- Detonate RCDs ---

    private void OnDetonateRcds(Entity<MalfunctionAiComponent> ent, ref MalfDetonateRcdsEvent args)
    {
        if (args.Handled || !TryComp<MalfRcdComponent>(ent.Owner, out var rcd))
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<RCDComponent, TransformComponent>();
        while (query.MoveNext(out var rcdUid, out _, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (HasComp<MalfPendingOverloadComponent>(rcdUid))
                continue;

            var pending = AddComp<MalfPendingOverloadComponent>(rcdUid);
            pending.TriggerAt = _timing.CurTime + rcd.Delay;
            pending.Intensity = rcd.Intensity;
            pending.MaxTileIntensity = rcd.MaxTileIntensity;
            pending.Slope = rcd.Slope;
            pending.Source = ent.Owner;

            _audio.PlayPvs(rcd.WarningSound, rcdUid);
            _popups.PopupEntity(Loc.GetString("malfunction-ai-popup-rcd-warning"), rcdUid, PopupType.LargeCaution);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-rcd-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Disable emergency lights ---

    private void OnDisableEmergencyLights(Entity<MalfunctionAiComponent> ent, ref MalfDisableEmergencyLightsEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<EmergencyLightComponent, TransformComponent>();
        while (query.MoveNext(out var lightUid, out _, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!TryComp<BatteryComponent>(lightUid, out var battery))
                continue;

            _battery.SetCharge((lightUid, battery), 0f);
            count++;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-emergency-lights-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }

    // --- Decrypt Syndicate keys ---

    private static readonly ProtoId<RadioChannelPrototype> SyndicateChannel = "Syndicate";

    private void OnDecryptSyndicateKeys(Entity<MalfunctionAiComponent> ent, ref MalfDecryptSyndicateKeysEvent args)
    {
        EnsureComp<ActiveRadioComponent>(ent.Owner).Channels.Add(SyndicateChannel);
        EnsureComp<IntrinsicRadioTransmitterComponent>(ent.Owner).Channels.Add(SyndicateChannel);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-decrypt-success"), ent.Owner);
    }
}

/// <summary>
/// Raised broadcast when a Malfunction AI arms the Doomsday device.
/// The Malfunction AI game rule listens for this and starts the countdown / blast.
/// </summary>
[ByRefEvent]
public readonly record struct MalfDoomsdayArmedEvent(EntityUid Ai);

