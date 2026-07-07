// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
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
    private void OnApcAltVerb(Entity<ApcComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (HasComp<MalfHackedApcComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-hack-apc"),
            Priority = 10,
            Act = () => TryHackApc((user, malf), target),
        });
    }

    private void OnMachineAltVerb(Entity<ApcPowerReceiverComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (!HasMalfAction(user, OverloadActionId))
            return;

        // Don't overload yourself or APCs (those get hacked instead).
        if (ent.Owner == user || HasComp<ApcComponent>(ent.Owner) || HasComp<MalfPendingOverloadComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-overload-machine"),
            Act = () => TryOverloadMachine((user, malf), target),
        });
    }

    private void OnCyborgAltVerb(Entity<BorgChassisComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        var user = args.User;
        if (!TryComp<MalfunctionAiComponent>(user, out var malf))
            return;

        if (!HasMalfAction(user, HackCyborgActionId))
            return;

        if (HasComp<MalfHackedCyborgComponent>(ent.Owner))
            return;

        var target = ent.Owner;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("malfunction-ai-verb-hack-cyborg"),
            Act = () => TryHackCyborg((user, malf), target),
        });
    }

    public bool TryHackApc(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (!HasComp<ApcComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
            return false;
        }

        if (HasComp<MalfHackedApcComponent>(target))
        {
            _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-apc-already-hacked"), ent.Owner);
            return false;
        }

        // Hacking grants processing power rather than costing it.
        AddComp<MalfHackedApcComponent>(target);
        ent.Comp.HackedApcCount++;
        Dirty(ent);

        // Refresh the APC screen right away: hacked APCs show the blue glitched (emag) screen.
        _apc.UpdateApcState(target);

        var gain = FixedPoint2.Zero;
        if (TryComp<StoreComponent>(ent.Owner, out var store))
        {
            var balance = GetBalance(ent, store);
            gain = FixedPoint2.Min(ent.Comp.MaxProcessingPower - balance, ent.Comp.CpuPerApc);
            if (gain > 0)
            {
                _store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { ent.Comp.Currency, gain } },
                    ent.Owner,
                    store);
            }

            SyncPowerMirror(ent, store);
        }

        _popups.PopupCursor(
            Loc.GetString("malfunction-ai-popup-hack-apc-success",
                ("gain", gain.Int()),
                ("power", ent.Comp.ProcessingPower.Int()),
                ("count", ent.Comp.HackedApcCount)),
            ent.Owner);
        return true;
    }

    private void OnOverloadMachine(Entity<MalfunctionAiComponent> ent, ref MalfOverloadMachineEvent args)
    {
        if (args.Handled)
            return;

        // Prefer the machine actually under the cursor, then fall back to whatever
        // valid machine is closest to the clicked point — never a random neighbour.
        if (args.Entity is { } hovered
            && IsValidOverloadTarget(ent.Owner, hovered)
            && TryOverloadMachine(ent, hovered))
        {
            args.Handled = true;
            return;
        }

        var clickPos = _transform.ToMapCoordinates(args.Target).Position;
        EntityUid? best = null;
        var bestDistance = float.MaxValue;

        foreach (var candidate in _lookup.GetEntitiesInRange(args.Target, 0.75f))
        {
            if (!IsValidOverloadTarget(ent.Owner, candidate))
                continue;

            var distance = (clickPos - _transform.GetWorldPosition(candidate)).Length();
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        if (best != null && TryOverloadMachine(ent, best.Value))
        {
            args.Handled = true;
            return;
        }

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-invalid-target"), ent.Owner);
    }

    private bool IsValidOverloadTarget(EntityUid user, EntityUid candidate)
    {
        return candidate != user
            && HasComp<ApcPowerReceiverComponent>(candidate)
            && !HasComp<ApcComponent>(candidate)
            && !HasComp<MalfPendingOverloadComponent>(candidate);
    }

    private bool TryOverloadMachine(Entity<MalfunctionAiComponent> ent, EntityUid target)
    {
        if (target == ent.Owner || HasComp<MalfPendingOverloadComponent>(target))
            return false;

        if (!TryComp<MalfOverloadComponent>(ent.Owner, out var overload))
            return false;

        var pending = AddComp<MalfPendingOverloadComponent>(target);
        pending.TriggerAt = _timing.CurTime + overload.Delay;
        pending.Intensity = overload.Intensity;
        pending.MaxTileIntensity = overload.MaxTileIntensity;
        pending.Slope = overload.Slope;
        pending.Source = ent.Owner;

        _audio.PlayPvs(overload.WarningSound, target);

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-overload-success"), ent.Owner);
        return true;
    }

    private void OnBlackout(Entity<MalfunctionAiComponent> ent, ref MalfBlackoutEvent args)
    {
        if (args.Handled)
            return;

        var gridUid = Transform(ent.Owner).GridUid;
        var count = 0;

        var query = EntityQueryEnumerator<ApcComponent, TransformComponent>();
        while (query.MoveNext(out var apcUid, out var apc, out var xform))
        {
            if (gridUid != null && xform.GridUid != gridUid)
                continue;

            if (!apc.MainBreakerEnabled)
                continue;

            _apc.ApcToggleBreaker(apcUid, apc);
            count++;
        }

        AnnounceFromAi(ent.Owner, Loc.GetString("malfunction-ai-announcement-blackout"));

        _popups.PopupCursor(Loc.GetString("malfunction-ai-popup-blackout-success", ("count", count)), ent.Owner);
        args.Handled = true;
    }
}
