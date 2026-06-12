// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Alert;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared._Funkystation.MalfAI;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Intellicard;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Manages the Malf AI doomsday countdown protocol.
/// Raises alert level, announces to the station, and triggers the ripple on completion.
/// </summary>
public sealed class MalfAiDoomsdaySystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiDoomsdayActionEvent>(OnStartDoomsday);

        // The countdown only runs while its initiator is operational: death, destruction
        // or being downloaded into an intellicard aborts the protocol.
        SubscribeLocalEvent<MalfAiDoomsdayComponent, MobStateChangedEvent>(OnDoomsdayMobState);
        SubscribeLocalEvent<MalfAiDoomsdayComponent, EntityTerminatingEvent>(OnDoomsdayTerminating);
        SubscribeLocalEvent<MalfAiDoomsdayComponent, EntGotInsertedIntoContainerMessage>(OnDoomsdayInserted);
        SubscribeLocalEvent<MalfAiDoomsdayComponent, EntGotRemovedFromContainerMessage>(OnDoomsdayRemovedFromCore);
    }

    private void OnDoomsdayRemovedFromCore(Entity<MalfAiDoomsdayComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        // The AI must stay in its core and protect it: leaving it (APC shunt, intellicard...)
        // aborts the protocol.
        if (HasComp<StationAiCoreComponent>(args.Container.Owner))
            AbortDoomsday(ent.Owner);
    }

    private void OnDoomsdayMobState(Entity<MalfAiDoomsdayComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            AbortDoomsday(ent.Owner);
    }

    private void OnDoomsdayTerminating(Entity<MalfAiDoomsdayComponent> ent, ref EntityTerminatingEvent args)
    {
        AbortDoomsday(ent.Owner);
    }

    private void OnDoomsdayInserted(Entity<MalfAiDoomsdayComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        // Carding the AI aborts the protocol; moving between APCs does not.
        if (HasComp<IntellicardComponent>(args.Container.Owner))
            AbortDoomsday(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MalfAiDoomsdayComponent, MalfAiMarkerComponent>();
        while (query.MoveNext(out var ent, out var doom, out _))
        {
            if (!doom.Active)
                continue;

            doom.RemainingTime -= TimeSpan.FromSeconds(frameTime);

            if (doom.RemainingTime <= TimeSpan.Zero)
            {
                CompleteDoomsday(ent, doom);
            }
        }
    }

    private void OnStartDoomsday(Entity<MalfAiMarkerComponent> ent, ref MalfAiDoomsdayActionEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<MalfAiDoomsdayComponent>(ent.Owner))
            return;

        // The protocol is bound to the AI core: it cannot be initiated from an APC.
        if (!HasComp<StationAiCoreComponent>(Transform(ent.Owner).ParentUid))
        {
            _popup.PopupEntity(Loc.GetString("malfai-doomsday-requires-core"), ent.Owner, ent.Owner);
            return;
        }

        var duration = _cfg.GetCVar(CCVars.MalfAiDoomsdayDuration);

        var doom = AddComp<MalfAiDoomsdayComponent>(ent.Owner);
        doom.Active = true;
        doom.RemainingTime = TimeSpan.FromSeconds(duration);

        var xform = Transform(ent.Owner);
        doom.Station = _station.GetStationInMap(xform.MapID);

        if (doom.Station is { } station)
        {
            _chat.DispatchStationAnnouncement(
                station,
                Loc.GetString("malfai-doomsday-announce"),
                Loc.GetString("malfai-doomsday-sender"),
                colorOverride: Color.Cyan);
        }

        EnsureComp<MalfAiDoomsdayMarkerComponent>(ent.Owner);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI {ToPrettyString(ent.Owner)} initiated Doomsday Protocol");

        var startEv = new MalfAiDoomsdayStartedEvent(doom.Station ?? default, ent.Owner);
        RaiseLocalEvent(startEv);

        args.Handled = true;
    }

    private void CompleteDoomsday(EntityUid ent, MalfAiDoomsdayComponent doom)
    {
        doom.Active = false;

        var completedEv = new MalfAiDoomsdayCompletedEvent(doom.Station ?? default, ent);
        RaiseLocalEvent(completedEv);

        EnsureComp<MalfAiDoomsdayCompletedComponent>(ent);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI {ToPrettyString(ent)} Doomsday Protocol completed!");
    }

    public void AbortDoomsday(EntityUid ent)
    {
        if (!TryComp<MalfAiDoomsdayComponent>(ent, out var doom))
            return;

        // Already finished or already aborted: nothing to stop.
        if (!doom.Active)
            return;

        doom.Active = false;
        RemComp<MalfAiDoomsdayComponent>(ent);
        RemComp<MalfAiDoomsdayMarkerComponent>(ent);

        _adminLog.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI {ToPrettyString(ent)} Doomsday Protocol ABORTED");

        if (doom.Station is { } station)
        {
            _chat.DispatchStationAnnouncement(
                station,
                Loc.GetString("malfai-doomsday-aborted"),
                Loc.GetString("malfai-doomsday-sender"),
                colorOverride: Color.Green);
        }
    }
}
