// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling;
using Content.Goobstation.Shared.Shadowling.Components.Abilities.PreAscension;
using Content.Server.Chat.Systems;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Shared.Actions;
using Content.Shared.Popups;

namespace Content.Goobstation.Server.Shadowling.Systems.Abilities.PreAscension;

/// <summary>
/// This handles Destroy Engines ability.
/// An ability that delays the evacuation shuttle by 10 minutes
/// </summary>
public sealed class ShadowlingDestroyEnginesSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergency = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingDestroyEnginesComponent, DestroyEnginesEvent>(OnDestroyEngines);
        SubscribeLocalEvent<ShadowlingDestroyEnginesComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<ShadowlingDestroyEnginesComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<ShadowlingDestroyEnginesComponent> ent, ref MapInitEvent args)
        => _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);

    private void OnShutdown(Entity<ShadowlingDestroyEnginesComponent> ent, ref ComponentShutdown args)
        => _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);

    private void OnDestroyEngines(EntityUid uid, ShadowlingDestroyEnginesComponent comp, DestroyEnginesEvent args)
    {
        if (args.Handled)
            return;

        var query = EntityQueryEnumerator<ShadowlingDestroyEnginesComponent>();
        while (query.MoveNext(out _, out var destroyEngines))
        {
            if (destroyEngines.HasBeenUsed)
            {
                _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-used"), uid);
                return;
            }
        }
        if (_emergency.EmergencyShuttleArrived)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-arrived"), uid);
            return;
        }

        if (_roundEnd.ExpectedCountdownEnd is null)
        {
            _popup.PopupEntity(Loc.GetString("shadowling-destroy-engines-not-called"), uid);
            return;
        }

        var message = string.Concat(Loc.GetString("shadowling-destroy-engines-message"),
            " ",
            Loc.GetString("shadowling-destroy-engines-delay", ("time", comp.DelayTime.TotalMinutes)));

        _chat.DispatchGlobalAnnouncement(message,
            Loc.GetString("shadowling-destroy-engines-sender"),
            colorOverride: Color.MediumPurple);

        // add sound
        comp.HasBeenUsed = true;

        _roundEnd.ExpectedCountdownEnd += comp.DelayTime;
        args.Handled = true;
        _actions.RemoveAction(args.Performer, (args.Action.Owner, args.Action.Comp));
    }
}
