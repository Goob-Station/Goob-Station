// SPDX-FileCopyrightText: 2023 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
// SPDX-FileCopyrightText: 2024 JustCone <141039037+JustCone14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2024 Moomoobeef <62638182+Moomoobeef@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 PopGamer46 <yt1popgamer@gmail.com>
// SPDX-FileCopyrightText: 2024 PursuitInAshes <pursuitinashes@gmail.com>
// SPDX-FileCopyrightText: 2024 QueerNB <176353696+QueerNB@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Saphire Lattice <lattice@saphi.re>
// SPDX-FileCopyrightText: 2024 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Anomaly.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Anomaly;

/// <summary>
/// A device that allows you to translate anomaly activity into multitool signals.
/// </summary>
public sealed partial class AnomalySynchronizerSystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAnomalySystem _anomaly = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDeviceLinkSystem _deviceLink = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomalySynchronizerComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<AnomalySynchronizerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<AnomalySynchronizerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<AnomalySynchronizerComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerbs);

        SubscribeLocalEvent<AnomalyPulseEvent>(OnAnomalyPulse);
        SubscribeLocalEvent<AnomalySeverityChangedEvent>(OnAnomalySeverityChanged);
        SubscribeLocalEvent<AnomalyStabilityChangedEvent>(OnAnomalyStabilityChanged);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<AnomalySynchronizerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var sync, out var synchronizerTransform))
        {
            if (sync.ConnectedAnomaly == null)
                continue;

            if (curTime < sync.NextCheckTime)
                continue;

            sync.NextCheckTime += sync.CheckFrequency;
            Dirty(uid, sync);

            if (TerminatingOrDeleted(sync.ConnectedAnomaly))
            {
                DisconnectFromAnomaly((uid, sync));
                continue;
            }

            // Use TryComp instead of Transform(uid) to take care of cases where the anomaly is out of
            // PVS range on the client, but the synchronizer isn't.
            if (!TryComp(sync.ConnectedAnomaly.Value, out TransformComponent? anomalyTransform))
                continue;

            if (anomalyTransform.MapUid != synchronizerTransform.MapUid)
            {
                DisconnectFromAnomaly((uid, sync));
                continue;
            }

            if (!synchronizerTransform.Coordinates.TryDistance(EntityManager, anomalyTransform.Coordinates, out var distance))
                continue;

            if (distance > sync.AttachRange)
                DisconnectFromAnomaly((uid, sync));
        }
    }

    /// <summary>
    /// If powered, try to attach a nearby anomaly.
    /// </summary>
    public bool TryAttachNearbyAnomaly(Entity<AnomalySynchronizerComponent> ent, EntityUid? user = null)
    {
        if (!_power.IsPowered(ent.Owner))
        {
            _popup.PopupClient(Loc.GetString("base-computer-ui-component-not-powered", ("machine", ent)), ent, user);
            return false;
        }

        var coords = _transform.GetMapCoordinates(ent);
        var anomaly = _entityLookup.GetEntitiesInRange<AnomalyComponent>(coords, ent.Comp.AttachRange).FirstOrDefault();

        if (anomaly.Owner is { Valid: false }) // no anomaly in range
        {
            _popup.PopupClient(Loc.GetString("anomaly-sync-no-anomaly"), ent, user);
            return false;
        }

        ConnectToAnomaly(ent, anomaly, user);
        return true;
    }

    private void OnPowerChanged(Entity<AnomalySynchronizerComponent> ent, ref PowerChangedEvent args)
    {
        if (args.Powered)
            return;

        if (ent.Comp.ConnectedAnomaly == null)
            return;

        DisconnectFromAnomaly(ent);
    }

    private void OnExamined(Entity<AnomalySynchronizerComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ConnectedAnomaly.HasValue ? "anomaly-sync-examine-connected" : "anomaly-sync-examine-not-connected"));
    }

    private void OnGetInteractionVerbs(Entity<AnomalySynchronizerComponent> ent, ref GetVerbsEvent<InteractionVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        var user = args.User;

        if (ent.Comp.ConnectedAnomaly == null)
        {
            args.Verbs.Add(new()
            {
                Act = () => TryAttachNearbyAnomaly(ent, user),
                Message = Loc.GetString("anomaly-sync-connect-verb-message", ("machine", ent)),
                Text = Loc.GetString("anomaly-sync-connect-verb-text"),
            });
        }
        else
        {
            args.Verbs.Add(new()
            {
                Act = () => DisconnectFromAnomaly(ent, user),
                Message = Loc.GetString("anomaly-sync-disconnect-verb-message", ("machine", ent)),
                Text = Loc.GetString("anomaly-sync-disconnect-verb-text"),
            });
        }
    }

    private void OnInteractHand(Entity<AnomalySynchronizerComponent> ent, ref InteractHandEvent args)
    {
        TryAttachNearbyAnomaly(ent, args.User);
    }

    private void ConnectToAnomaly(Entity<AnomalySynchronizerComponent> ent, Entity<AnomalyComponent> anomaly, EntityUid? user = null)
    {
        if (ent.Comp.ConnectedAnomaly == anomaly)
            return;

        ent.Comp.ConnectedAnomaly = anomaly;
        Dirty(ent);
        //move the anomaly to the center of the synchronizer, for aesthetics.
        var targetXform = _transform.GetWorldPosition(ent);
        _transform.SetWorldPosition(anomaly, targetXform);

        if (ent.Comp.PulseOnConnect)
            _anomaly.DoAnomalyPulse(anomaly, anomaly);

        _popup.PopupPredicted(Loc.GetString("anomaly-sync-connected"), ent, user, PopupType.Medium);
        _audio.PlayPredicted(ent.Comp.ConnectedSound, ent, user);
    }

    //TODO: disconnection from the anomaly should also be triggered if the anomaly is far away from the synchronizer.
    //Currently only bluespace anomaly can do this, but for some reason it is the only one that cannot be connected to the synchronizer.
    private void DisconnectFromAnomaly(Entity<AnomalySynchronizerComponent> ent, EntityUid? user = null)
    {
        if (ent.Comp.ConnectedAnomaly == null)
            return;

        if (ent.Comp.PulseOnDisconnect && TryComp<AnomalyComponent>(ent.Comp.ConnectedAnomaly, out var anomaly))
        {
            _anomaly.DoAnomalyPulse(ent.Comp.ConnectedAnomaly.Value, anomaly);
        }

        _popup.PopupPredicted(Loc.GetString("anomaly-sync-disconnected"), ent, user, PopupType.Large);
        _audio.PlayPredicted(ent.Comp.DisconnectedSound, ent, user);

        ent.Comp.ConnectedAnomaly = null;
        Dirty(ent);
    }

    private void OnAnomalyPulse(ref AnomalyPulseEvent args)
    {
        var query = EntityQueryEnumerator<AnomalySynchronizerComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (args.Anomaly != component.ConnectedAnomaly)
                continue;

            if (!_power.IsPowered(uid))
                continue;

            _deviceLink.InvokePort(uid, component.PulsePort);
        }
    }

    private void OnAnomalySeverityChanged(ref AnomalySeverityChangedEvent args)
    {
        var query = EntityQueryEnumerator<AnomalySynchronizerComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (args.Anomaly != component.ConnectedAnomaly)
                continue;

            if (!_power.IsPowered(uid))
                continue;

            //The superscritical port is invoked not at the AnomalySupercriticalEvent,
            //but at the moment the growth animation starts. Otherwise, there is no point in this port.
            //ATTENTION! the console command supercriticalanomaly does not work here,
            //as it forcefully causes growth to start without increasing severity.
            if (args.Severity >= 1)
                _deviceLink.InvokePort(uid, component.SupercritPort);
        }
    }

    private void OnAnomalyStabilityChanged(ref AnomalyStabilityChangedEvent args)
    {
        var anomaly = Comp<AnomalyComponent>(args.Anomaly);

        var query = EntityQueryEnumerator<AnomalySynchronizerComponent>();
        while (query.MoveNext(out var uid, out var sync))
        {
            if (sync.ConnectedAnomaly != args.Anomaly)
                continue;

            if (!_power.IsPowered(uid))
                continue;

            if (args.Stability < anomaly.DecayThreshold)
            {
                _deviceLink.InvokePort(uid, sync.DecayingPort);
            }
            else if (args.Stability > anomaly.GrowthThreshold)
            {
                _deviceLink.InvokePort(uid, sync.GrowingPort);
            }
            else
            {
                _deviceLink.InvokePort(uid, sync.StabilizePort);
            }
        }
    }
}
