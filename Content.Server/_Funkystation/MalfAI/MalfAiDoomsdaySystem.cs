// SPDX-FileCopyrightText: 2025 Dreykor <arguemeu@gmail.com>
// SPDX-FileCopyrightText: 2025 GabyChangelog <agentepanela2@gmail.com>
// SPDX-FileCopyrightText: 2025 Kyoth25f <kyoth25f@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 funkystationbot <funky@funkystation.org>
//
// SPDX-License-Identifier: MIT

using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared._Funkystation.MalfAI.Actions;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Content.Shared.Audio;
using Robust.Shared.Random;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Silicons.StationAi;
using Content.Shared._Funkystation.MalfAI.Events;
using Content.Shared._Gabystation.MalfAi.Components;
using Content.Server._Funkystation.MalfAI.Components;
using Content.Shared._Funkystation.CCVar;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Handles the Malf AI Doomsday Protocol activation, countdown, announcements, and cancellation.
/// </summary>
public sealed class MalfAiDoomsdaySystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly AlertLevelSystem _alerts = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;

    private const string DoomsdayAlertLevel = "doomsday";
    private const float DoomsdaySongBuffer = 1.5f; // seconds before alert

    public override void Initialize()
    {
        base.Initialize();

        // Action trigger on the AI entity (same pattern as shunt system).
        SubscribeLocalEvent<StationAiHeldComponent, MalfAiDoomsdayActionEvent>(OnDoomsdayAction);

        // Cancel when AI is removed from its core container.
        SubscribeLocalEvent<MalfAiDoomsdayComponent, EntRemovedFromContainerMessage>(OnEntRemovedFromContainer);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<MalfAiDoomsdayComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Active)
                continue;

            // Validate AI still in the core holder we recorded.
            if (!StillInRecordedCore(uid, comp))
            {
                AbortDoomsday(uid, comp, "malfai-doomsday-abort-left-core");
                continue;
            }

            // Check if we should start the doomsday music
            if (!comp.MusicStarted && comp.SongDuration.HasValue && comp.SelectedDoomsdaySong != null)
            {
                // Start music when remaining time matches song duration (with small buffer)
                var songDurationWithBuffer = TimeSpan.FromSeconds(comp.SongDuration.Value + DoomsdaySongBuffer);
                if (comp.RemainingTime <= songDurationWithBuffer)
                {
                    _sound.DispatchStationEventMusic(comp.Station, comp.SelectedDoomsdaySong, StationEventMusicType.Doomsday);
                    comp.MusicStarted = true;
                }
            }

            // Store previous time for threshold detection
            var prevTime = comp.RemainingTime;

            // Tick countdown.
            comp.RemainingTime -= TimeSpan.FromSeconds(frameTime);

            if (comp.RemainingTime <= TimeSpan.Zero)
            {
                comp.RemainingTime = TimeSpan.Zero;
                comp.Active = false;
                // Stop doomsday protocol music
                StopDoomsdayMusic(comp.Station);
                RaiseLocalEvent(new MalfAiDoomsdayCompletedEvent(comp.Station, uid));
                RemCompDeferred<MalfAiDoomsdayComponent>(uid);
                continue;
            }

            // Check for threshold crossings and announce
            CheckAndAnnounceThresholds(uid, prevTime, comp.RemainingTime);
        }
    }

    private void OnDoomsdayAction(Entity<StationAiHeldComponent> ai, ref MalfAiDoomsdayActionEvent args)
    {
        // Only malf AIs can use this.
        if (!HasComp<MalfunctioningAiComponent>(ai))
        {
            ShowDoomsdayPopup(ai.Owner, "malfai-doomsday-popup-not-malf");
            return;
        }

        // Ensure not already active.
        if (TryComp<MalfAiDoomsdayComponent>(ai, out var existing) && existing.Active)
        {
            ShowDoomsdayPopup(ai.Owner, "malfai-doomsday-popup-already-active");
            args.Handled = true;
            return;
        }

        // Must be held in a container, and that holder must be an AI core.
        if (!_containers.TryGetContainingContainer((ai.Owner, null, null), out var container) || container is not ContainerSlot)
        {
            ShowDoomsdayPopup(ai.Owner, "malfai-doomsday-popup-need-core");
            args.Handled = true;
            return;
        }

        var holder = container.Owner;
        if (!HasComp<StationAiCoreComponent>(holder))
        {
            ShowDoomsdayPopup(ai.Owner, "malfai-doomsday-popup-need-core");
            args.Handled = true;
            return;
        }

        // Determine owning station.
        var station = _station.GetOwningStation(ai.Owner);
        if (station == null)
        {
            ShowDoomsdayPopup(ai.Owner, "malfai-doomsday-popup-no-station");
            args.Handled = true;
            return;
        }

        // Get duration configuration
        var duration = _cfg.GetCVar(CCVarsMalfAi.MalfAiDoomsdayDuration);
        var actualDuration = duration > 0 ? duration : 1f; // Ensure positive duration

        // Record previous alert level state.
        var comp = EnsureComp<MalfAiDoomsdayComponent>(ai);
        InitializeDoomsdayComponent(comp, station.Value, holder, actualDuration);

        // Switch to Cyan and lock it.
        _alerts.SetLevel(station.Value, DoomsdayAlertLevel, playSound: true, announce: true, force: true, locked: true);

        // Initialize doomsday music
        SetupDoomsdayMusic(comp);

        // Initial announcement.
        AnnounceRemaining(ai.Owner, comp.RemainingTime, initial: true);

        // Raise event to notify systems that doomsday has started
        RaiseLocalEvent(new MalfAiDoomsdayStartedEvent(station.Value, ai.Owner));

        args.Handled = true;
    }

    private void OnEntRemovedFromContainer(Entity<MalfAiDoomsdayComponent> ent, ref EntRemovedFromContainerMessage msg)
    {
        // Only care about removal from the recorded core holder container.
        if (!ent.Comp.Active || msg.Container.Owner != ent.Comp.CoreHolder)
            return;

        AbortDoomsday(ent, ent.Comp, "malfai-doomsday-abort-left-core");
    }

    private bool StillInRecordedCore(EntityUid ai, MalfAiDoomsdayComponent comp)
    {
        if (!HasComp<StationAiHeldComponent>(ai))
            return false;
        if (!_containers.TryGetContainingContainer((ai, null, null), out var container))
            return false;
        return container.Owner == comp.CoreHolder && HasComp<StationAiCoreComponent>(comp.CoreHolder);
    }

    public void AbortDoomsday(EntityUid uid, MalfAiDoomsdayComponent comp, string reason)
    {
        comp.Active = false;
        StopDoomsdayMusic(comp.Station);
        comp.SelectedDoomsdaySong = null;
        comp.MusicStarted = false;
        comp.SongDuration = null;

        // Revert alert level if we have a prior level.
        if (comp.Station != default && !string.IsNullOrEmpty(comp.PrevAlertLevel))
        {
            _alerts.SetLevel(comp.Station, comp.PrevAlertLevel, playSound: true, announce: true, force: true, locked: comp.PrevLocked);
        }

        AnnounceLoc(uid, reason, urgent: true);

        RemCompDeferred<MalfAiDoomsdayComponent>(uid);
    }

    private void CheckAndAnnounceThresholds(EntityUid ai, TimeSpan prevTime, TimeSpan currentTime)
    {
        // Check for minute thresholds (every 60 seconds)
        var prevMinutes = (int) Math.Floor(prevTime.TotalMinutes);
        var currentMinutes = (int) Math.Floor(currentTime.TotalMinutes);

        if (prevMinutes > currentMinutes && currentMinutes >= 0)
        {
            var minuteThreshold = TimeSpan.FromMinutes(currentMinutes + 1);
            AnnounceRemaining(ai, minuteThreshold, initial: false);
            return; // Only one announcement per frame
        }

        // Check special final thresholds (30s and final 5 seconds)
        var specialThresholds = new[] { 30, 5, 4, 3, 2, 1 };

        foreach (var threshold in specialThresholds)
        {
            var thresholdTime = TimeSpan.FromSeconds(threshold);
            if (prevTime > thresholdTime && currentTime <= thresholdTime)
            {
                AnnounceRemaining(ai, thresholdTime, initial: false);
                return; // Only one announcement per frame
            }
        }
    }

    private void AnnounceRemaining(EntityUid ai, TimeSpan remainingTime, bool initial = false)
    {
        var timeStr = remainingTime.ToString(@"mm\:ss");
        var key = initial
            ? "malfai-doomsday-announce-initial"
            : "malfai-doomsday-announce-progress";

        AnnounceLoc(ai, key, urgent: initial || remainingTime.TotalSeconds <= 5, ("time", timeStr));
    }

    private void Announce(EntityUid ai, string text, bool urgent = false)
    {
        // Announce to the station; this will route to appropriate receivers.
        _chat.DispatchStationAnnouncement(
            ai,
            text,
            sender: Loc.GetString("malfai-doomsday-sender"),
            playDefaultSound: true,
            colorOverride: urgent ? Color.Red : Color.Cyan);
    }

    private void AnnounceLoc(EntityUid ai, string key, bool urgent = false, params (string, object)[] args)
    {
        var text = Loc.GetString(key, args);
        Announce(ai, text, urgent);
    }

    private void StopDoomsdayMusic(EntityUid station)
    {
        _sound.StopStationEventMusic(station, StationEventMusicType.Doomsday);
    }

    /// <summary>
    /// Gets the AI eye entity for popup positioning, falls back to core if eye unavailable
    /// </summary>
    private EntityUid? GetAiEyeForPopup(EntityUid aiUid)
    {
        if (!_stationAi.TryGetCore(aiUid, out var core) || core.Comp?.RemoteEntity == null)
            return null;

        return core.Comp.RemoteEntity.Value;
    }

    /// <summary>
    /// Shows a popup message to the AI, handling eye positioning automatically
    /// </summary>
    private void ShowDoomsdayPopup(EntityUid aiUid, string message)
    {
        var popupTarget = GetAiEyeForPopup(aiUid) ?? aiUid;
        _popup.PopupEntity(Loc.GetString(message), popupTarget, aiUid);
    }

    /// <summary>
    /// Initializes all component properties for a new doomsday activation
    /// </summary>
    private void InitializeDoomsdayComponent(MalfAiDoomsdayComponent comp, EntityUid station, EntityUid holder, float duration)
    {
        comp.Active = true;
        comp.Station = station;
        comp.CoreHolder = holder;
        comp.RemainingTime = TimeSpan.FromSeconds(duration);

        // Store previous alert level state
        if (TryComp<AlertLevelComponent>(station, out var alert))
        {
            comp.PrevAlertLevel = alert.CurrentLevel;
            comp.PrevLocked = alert.IsLevelLocked;
        }
        else
        {
            comp.PrevAlertLevel = string.Empty;
            comp.PrevLocked = false;
        }
    }

    /// <summary>
    /// Sets up doomsday music selection and timing
    /// </summary>
    private void SetupDoomsdayMusic(MalfAiDoomsdayComponent comp)
    {
        comp.SelectedDoomsdaySong = null;
        comp.SongDuration = null;
        comp.MusicStarted = false;

        if (!_prototypeManager.TryIndex<SoundCollectionPrototype>("DoomsdayMusic", out var collection) ||
            collection.PickFiles.Count == 0)
            return;

        var randomIndex = _random.Next(collection.PickFiles.Count);
        var selectedFile = collection.PickFiles[randomIndex];
        var specifier = new SoundPathSpecifier(selectedFile);
        comp.SelectedDoomsdaySong = _audio.ResolveSound(specifier);

        // Try to get the song duration for timing
        try
        {
            var songDuration = _audio.GetAudioLength(comp.SelectedDoomsdaySong);
            comp.SongDuration = songDuration.TotalSeconds > 0 ? (float) songDuration.TotalSeconds : null;
        }
        catch (Exception)
        {
            // If duration can't be determined, music timing won't work but that's acceptable
            comp.SongDuration = null;
        }
    }
}
