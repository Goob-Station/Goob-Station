// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 v0id <>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.BloodCult.Components;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Popups;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Dataset;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class ShuttleCurseSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleCurseComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnActivate(Entity<ShuttleCurseComponent> orb, ref ActivateInWorldEvent args)
    {
        if (args.Handled ||
            (!HasComp<BloodCultistComponent>(args.User) && !HasComp<BloodCultConstructComponent>(args.User)))
            return;

        args.Handled = true;

        if (_bloodCultRule.GetShuttleCurseCharges() <= 0)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-max-charges"), orb, args.User);
            return;
        }

        if (_emergencyShuttle.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-arrived"), orb, args.User);
            return;
        }

        if (!_roundEnd.DelayShuttle(orb.Comp.DelayTime))
        {
            _popup.PopupEntity(Loc.GetString("shuttle-curse-shuttle-not-called"), orb, args.User);
            return;
        }

        if (!_bloodCultRule.TryConsumeShuttleCurseCharge())
            return;

        var message = string.Empty;
        if (_prototype.TryIndex(orb.Comp.CurseMessages, out LocalizedDatasetPrototype? messages))
            message = _random.Pick(messages.Values);

        _chat.DispatchGlobalAnnouncement(
            Loc.GetString(
                "shuttle-curse-success-global",
                ("message", message),
                ("time", orb.Comp.DelayTime.TotalMinutes)),
            Loc.GetString("shuttle-curse-system-failure"),
            colorOverride: Color.Gold);

        _popup.PopupEntity(Loc.GetString("shuttle-curse-success"), orb, args.User);
        _audio.PlayPvs(orb.Comp.ScatterSound, orb);
        QueueDel(orb);
    }
}
