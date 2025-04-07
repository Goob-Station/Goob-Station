// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Alzore <140123969+Blackern5000@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Brandon Hu <103440971+Brandon-Huu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <65184747+Dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dimastra <dimastra@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <51352440+JIPDawg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 JIPDawg <JIPDawg93@gmail.com>
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
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Thomas <87614336+Aeshus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 c4llv07e <38111072+c4llv07e@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 github-actions[bot] <41898282+github-actions[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lunarcomets <140772713+lunarcomets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 lzk <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 stellar-novas <stellar_novas@riseup.net>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Ghost;
using Content.Server.Hands.Systems;
using Content.Server.Inventory;
using Content.Server.Popups;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Systems;
using Content.Shared.Bed.Cryostorage;
using Content.Shared.Chat;
using Content.Shared.Climbing.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Mind.Components;
using Content.Shared.StationRecords;
using Content.Shared.UserInterface;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.Bed.Cryostorage;

/// <inheritdoc/>
public sealed class CryostorageSystem : SharedCryostorageSystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly AccessReaderSystem _accessReader = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly GhostSystem _ghostSystem = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly ServerInventorySystem _inventory = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryostorageComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpened);
        SubscribeLocalEvent<CryostorageComponent, CryostorageRemoveItemBuiMessage>(OnRemoveItemBuiMessage);

        SubscribeLocalEvent<CryostorageContainedComponent, PlayerSpawnCompleteEvent>(OnPlayerSpawned);
        SubscribeLocalEvent<CryostorageContainedComponent, MindRemovedMessage>(OnMindRemoved);

        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playerManager.PlayerStatusChanged -= PlayerStatusChanged;
    }

    private void OnBeforeUIOpened(Entity<CryostorageComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        UpdateCryostorageUIState(ent);
    }

    private void OnRemoveItemBuiMessage(Entity<CryostorageComponent> ent, ref CryostorageRemoveItemBuiMessage args)
    {
        var (_, comp) = ent;
        var attachedEntity = args.Actor;
        var cryoContained = GetEntity(args.StoredEntity);

        if (!comp.StoredPlayers.Contains(cryoContained) || !IsInPausedMap(cryoContained))
            return;

        if (!HasComp<HandsComponent>(attachedEntity))
            return;

        if (!_accessReader.IsAllowed(attachedEntity, ent))
        {
            _popup.PopupEntity(Loc.GetString("cryostorage-popup-access-denied"), attachedEntity, attachedEntity);
            return;
        }

        EntityUid? entity = null;
        if (args.Type == CryostorageRemoveItemBuiMessage.RemovalType.Hand)
        {
            if (_hands.TryGetHand(cryoContained, args.Key, out var hand))
                entity = hand.HeldEntity;
        }
        else
        {
            if (_inventory.TryGetSlotContainer(cryoContained, args.Key, out var slot, out _))
                entity = slot.ContainedEntity;
        }

        if (entity == null)
            return;

        AdminLog.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(attachedEntity):player} removed item {ToPrettyString(entity)} from cryostorage-contained player " +
            $"{ToPrettyString(cryoContained):player}, stored in cryostorage {ToPrettyString(ent)}");

        _container.TryRemoveFromContainer(entity.Value);
        _transform.SetCoordinates(entity.Value, Transform(attachedEntity).Coordinates);
        _hands.PickupOrDrop(attachedEntity, entity.Value);
        UpdateCryostorageUIState(ent);
    }

    private void UpdateCryostorageUIState(Entity<CryostorageComponent> ent)
    {
        var state = new CryostorageBuiState(GetAllContainedData(ent));
        _ui.SetUiState(ent.Owner, CryostorageUIKey.Key, state);
    }

    private void OnPlayerSpawned(Entity<CryostorageContainedComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        // if you spawned into cryostorage, we're not gonna round-remove you.
        ent.Comp.GracePeriodEndTime = null;
    }

    private void OnMindRemoved(Entity<CryostorageContainedComponent> ent, ref MindRemovedMessage args)
    {
        var comp = ent.Comp;

        if (!TryComp<CryostorageComponent>(comp.Cryostorage, out var cryostorageComponent))
            return;

        if (comp.GracePeriodEndTime != null)
            comp.GracePeriodEndTime = Timing.CurTime + cryostorageComponent.NoMindGracePeriod;
        comp.AllowReEnteringBody = false;
        comp.UserId = args.Mind.Comp.UserId;
    }

    private void PlayerStatusChanged(object? sender, SessionStatusEventArgs args)
    {
        if (args.Session.AttachedEntity is not { } entity)
            return;

        if (!TryComp<CryostorageContainedComponent>(entity, out var containedComponent))
            return;

        if (args.NewStatus is SessionStatus.Disconnected or SessionStatus.Zombie)
        {
            containedComponent.AllowReEnteringBody = true;
            var delay = CompOrNull<CryostorageComponent>(containedComponent.Cryostorage)?.NoMindGracePeriod ?? TimeSpan.Zero;
            containedComponent.GracePeriodEndTime = Timing.CurTime + delay;
            containedComponent.UserId = args.Session.UserId;
        }
        else if (args.NewStatus == SessionStatus.InGame)
        {
            HandleCryostorageReconnection((entity, containedComponent));
        }
    }

    public void HandleEnterCryostorage(Entity<CryostorageContainedComponent> ent, NetUserId? userId)
    {
        var comp = ent.Comp;
        var cryostorageEnt = ent.Comp.Cryostorage;

        var station = _station.GetOwningStation(ent);
        var name = Name(ent.Owner);

        if (!TryComp<CryostorageComponent>(cryostorageEnt, out var cryostorageComponent))
            return;

        // if we have a session, we use that to add back in all the job slots the player had.
        if (userId != null)
        {
            foreach (var uniqueStation in _station.GetStationsSet())
            {
                if (!TryComp<StationJobsComponent>(uniqueStation, out var stationJobs))
                    continue;

                if (!_stationJobs.TryGetPlayerJobs(uniqueStation, userId.Value, out var jobs, stationJobs))
                    continue;

                foreach (var job in jobs)
                {
                    _stationJobs.TryAdjustJobSlot(uniqueStation, job, 1, clamp: true);
                }

                _stationJobs.TryRemovePlayerJobs(uniqueStation, userId.Value, stationJobs);
            }
        }

        _audio.PlayPvs(cryostorageComponent.RemoveSound, ent);

        EnsurePausedMap();
        if (PausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        if (!CryoSleepRejoiningEnabled || !comp.AllowReEnteringBody)
        {
            if (userId != null && Mind.TryGetMind(userId.Value, out var mind) &&
                HasComp<CryostorageContainedComponent>(mind.Value.Comp.CurrentEntity))
            {
                _ghostSystem.OnGhostAttempt(mind.Value, false);
            }
        }

        comp.AllowReEnteringBody = false;
        _transform.SetParent(ent, PausedMap.Value);
        cryostorageComponent.StoredPlayers.Add(ent);
        Dirty(ent, comp);
        UpdateCryostorageUIState((cryostorageEnt.Value, cryostorageComponent));
        AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(ent):player} was entered into cryostorage inside of {ToPrettyString(cryostorageEnt.Value)}");

        if (!TryComp<StationRecordsComponent>(station, out var stationRecords))
            return;

        var jobName = Loc.GetString("earlyleave-cryo-job-unknown");
        var recordId = _stationRecords.GetRecordByName(station.Value, name);
        if (recordId != null)
        {
            var key = new StationRecordKey(recordId.Value, station.Value);
            if (_stationRecords.TryGetRecord<GeneralStationRecord>(key, out var entry, stationRecords))
                jobName = entry.JobTitle;

            _stationRecords.RemoveRecord(key, stationRecords);
        }

        _chatSystem.DispatchStationAnnouncement(station.Value,
            Loc.GetString(
                "earlyleave-cryo-announcement",
                ("character", name),
                ("entity", ent.Owner), // gender things for supporting downstreams with other languages
                ("job", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(jobName))
            ), Loc.GetString("earlyleave-cryo-sender"),
            playDefaultSound: false
        );
    }

    private void HandleCryostorageReconnection(Entity<CryostorageContainedComponent> entity)
    {
        var (uid, comp) = entity;
        if (!CryoSleepRejoiningEnabled || !IsInPausedMap(uid))
            return;

        // how did you destroy these? they're indestructible.
        if (comp.Cryostorage is not { } cryostorage ||
            TerminatingOrDeleted(cryostorage) ||
            !TryComp<CryostorageComponent>(cryostorage, out var cryostorageComponent))
        {
            QueueDel(entity);
            return;
        }

        var cryoXform = Transform(cryostorage);
        _transform.SetParent(uid, cryoXform.ParentUid);
        _transform.SetCoordinates(uid, cryoXform.Coordinates);
        if (!_container.TryGetContainer(cryostorage, cryostorageComponent.ContainerId, out var container) ||
            !_container.Insert(uid, container, cryoXform))
        {
            _climb.ForciblySetClimbing(uid, cryostorage);
        }

        comp.GracePeriodEndTime = null;
        cryostorageComponent.StoredPlayers.Remove(uid);
        AdminLog.Add(LogType.Action, LogImpact.High, $"{ToPrettyString(entity):player} re-entered the game from cryostorage {ToPrettyString(cryostorage)}");
        UpdateCryostorageUIState((cryostorage, cryostorageComponent));
    }

    protected override void OnInsertedContainer(Entity<CryostorageComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var (uid, comp) = ent;
        if (args.Container.ID != comp.ContainerId)
            return;

        base.OnInsertedContainer(ent, ref args);

        var locKey = CryoSleepRejoiningEnabled
            ? "cryostorage-insert-message-temp"
            : "cryostorage-insert-message-permanent";

        var msg = Loc.GetString(locKey, ("time", comp.GracePeriod.TotalMinutes));
        if (TryComp<ActorComponent>(args.Entity, out var actor))
            _chatManager.ChatMessageToOne(ChatChannel.Server, msg, msg, uid, false, actor.PlayerSession.Channel);
    }

    private List<CryostorageContainedPlayerData> GetAllContainedData(Entity<CryostorageComponent> ent)
    {
        var data = new List<CryostorageContainedPlayerData>();
        data.EnsureCapacity(ent.Comp.StoredPlayers.Count);

        foreach (var contained in ent.Comp.StoredPlayers)
        {
            data.Add(GetContainedData(contained));
        }

        return data;
    }

    private CryostorageContainedPlayerData GetContainedData(EntityUid uid)
    {
        var data = new CryostorageContainedPlayerData();
        data.PlayerName = Name(uid);
        data.PlayerEnt = GetNetEntity(uid);

        var enumerator = _inventory.GetSlotEnumerator(uid);
        while (enumerator.NextItem(out var item, out var slotDef))
        {
            data.ItemSlots.Add(slotDef.Name, Name(item));
        }

        foreach (var hand in _hands.EnumerateHands(uid))
        {
            if (hand.HeldEntity == null)
                continue;

            data.HeldItems.Add(hand.Name, Name(hand.HeldEntity.Value));
        }

        return data;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CryostorageContainedComponent>();
        while (query.MoveNext(out var uid, out var containedComp))
        {
            if (containedComp.GracePeriodEndTime == null)
                continue;

            if (Timing.CurTime < containedComp.GracePeriodEndTime)
                continue;

            Mind.TryGetMind(uid, out _, out var mindComp);
            var id = mindComp?.UserId ?? containedComp.UserId;
            HandleEnterCryostorage((uid, containedComp), id);
        }
    }
}