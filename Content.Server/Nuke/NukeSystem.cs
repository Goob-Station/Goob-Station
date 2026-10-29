// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.AlertLevel;
using Content.Server.Audio;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Kitchen.Components;
using Content.Server.Pinpointer;
using Content.Server.Popups;
using Content.Server.Station.Systems;
using Content.Shared.Audio;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Destructible; // Goobstation
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Item;
using Content.Shared.Kitchen;
using Content.Shared.Maps;
using Content.Shared.Nuke;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Shared.Timing;

namespace Content.Server.Nuke;

public sealed partial class NukeSystem : EntitySystem // Goob
{
    [Dependency] private readonly AlertLevelSystem _alertLevel = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosions = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly PointLightSystem _pointLight = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!; // Goobstation
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private float _nukeSongLength;
    private ResolvedSoundSpecifier _selectedNukeSong = String.Empty;

    private const float NukeSongBuffer = 1.5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NukeComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<NukeComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<NukeComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<NukeComponent, EntInsertedIntoContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<NukeComponent, EntRemovedFromContainerMessage>(OnItemSlotChanged);
        SubscribeLocalEvent<NukeComponent, ExaminedEvent>(OnExaminedEvent);

        SubscribeLocalEvent<NukeComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<NukeComponent, NukeAnchorMessage>(OnAnchorButtonPressed);
        SubscribeLocalEvent<NukeComponent, NukeArmedMessage>(OnArmButtonPressed);
        SubscribeLocalEvent<NukeComponent, NukeKeypadMessage>(OnKeypadButtonPressed);
        SubscribeLocalEvent<NukeComponent, NukeKeypadClearMessage>(OnClearButtonPressed);
        SubscribeLocalEvent<NukeComponent, NukeKeypadEnterMessage>(OnEnterButtonPressed);

        SubscribeLocalEvent<NukeComponent, NukeDisarmDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<NukeComponent, DestructionEventArgs>(OnNukeDestroyed); // Goobstation
        SubscribeLocalEvent<NukeComponent, ItemSlotInsertAttemptEvent>(OnDiskInsertAttempt); // Goobstation

        SubscribeLocalEvent<NukeDiskComponent, BeingMicrowavedEvent>(OnMicrowaved);
    }

    private void OnInit(EntityUid uid, NukeComponent component, ComponentInit args)
    {
        _itemSlots.AddItemSlot(uid, SharedNukeComponent.NukeDiskSlotId, component.DiskSlot);

        UpdateStatus(uid, component);
        UpdateUserInterface(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NukeComponent>();
        while (query.MoveNext(out var uid, out var nuke))
        {
            switch (nuke.Status)
            {
                case NukeStatus.ARMED:
                    TickTimer(uid, frameTime, nuke);
                    break;
                case NukeStatus.COOLDOWN:
                    TickCooldown(uid, frameTime, nuke);
                    break;
            }
        }
    }

    private void OnMapInit(EntityUid uid, NukeComponent nuke, MapInitEvent args)
    {
        nuke.RemainingTime = nuke.Timer;
        var originStation = _station.GetOwningStation(uid);

        if (originStation != null)
        {
            nuke.OriginStation = originStation;
        }
        else
        {
            var transform = Transform(uid);
            nuke.OriginMapGrid = (transform.MapID, transform.GridUid);
        }

        nuke.Code = GenerateRandomNumberString(nuke.CodeLength);
    }

    private void OnMicrowaved(Entity<NukeDiskComponent> ent, ref BeingMicrowavedEvent args)
    {
        if (ent.Comp.TimeModifier != null)
            return;

        var seconds = _random.NextGaussian(ent.Comp.MicrowaveMean.TotalSeconds, ent.Comp.MicrowaveStd.TotalSeconds);
        ent.Comp.TimeModifier = TimeSpan.FromSeconds(seconds);
        _popups.PopupEntity(Loc.GetString("nuke-disk-component-microwave"), ent.Owner, PopupType.Medium);
    }

    private void OnRemove(EntityUid uid, NukeComponent component, ComponentRemove args)
    {
        _itemSlots.RemoveItemSlot(uid, component.DiskSlot);
    }

    private void OnItemSlotChanged(EntityUid uid, NukeComponent component, ContainerModifiedMessage args)
    {
        if (!component.Initialized)
            return;

        if (args.Container.ID != component.DiskSlot.ID)
            return;

        // Goobstation start - nuke ops siege rework
        if (args is EntInsertedIntoContainerMessage &&
            component.Converted &&
            component.DiskSlot.Item is { } disk &&
            HasComp<NukeDiskComponent>(disk))
        {
            if (IsAntagonist(component.LastDiskInserter))
            {
                if (component.Status == NukeStatus.ARMED)
                {
                    component.RemainingTime = MathF.Max(
                        component.RemainingTime - component.DiskTimerReduction,
                        component.DiskReductionMinTime);
                    _popups.PopupEntity(Loc.GetString("nuke-component-disk-accelerated"), uid, PopupType.LargeCaution);
                }
            }
            else if (component.Status == NukeStatus.ARMED)
            {
                DisarmBomb(uid, component);
                _popups.PopupEntity(Loc.GetString("nuke-component-disk-defused"), uid, PopupType.Large);
            }
            else
            {
                component.Converted = false;
                component.Status = NukeStatus.AWAIT_DISK;

                var modifier = CompOrNull<NukeDiskComponent>(disk)?.TimeModifier ?? TimeSpan.Zero;
                component.RemainingTime = MathF.Max(component.Timer + (float) modifier.TotalSeconds, component.MinimumTime);

                _popups.PopupEntity(Loc.GetString("nuke-component-disk-recaptured"), uid, PopupType.Large);
            }

            component.LastDiskInserter = null;
        }
        // Goobstation end

        UpdateStatus(uid, component);
        UpdateUserInterface(uid, component);
    }

    #region Anchor

    private void OnAnchorChanged(EntityUid uid, NukeComponent component, ref AnchorStateChangedEvent args)
    {
        UpdateUserInterface(uid, component);

        if (args.Anchored == false && component.Status == NukeStatus.ARMED && component.RemainingTime > component.DisarmDoAfterLength)
        {
            DisarmBomb(uid, component);
        }

        UpdateAppearance(uid, component);
    }

    #endregion

    #region UI Events

    private async void OnAnchorButtonPressed(EntityUid uid, NukeComponent component, NukeAnchorMessage args)
    {
        if (component.Status == NukeStatus.ARMED)
            return;

        // Nuke has to have the disk in it to be moved, unless it was converted with a kit (Goobstation)
        if (!component.DiskSlot.HasItem && !component.Converted)
        {
            var msg = Loc.GetString("nuke-component-cant-anchor-toggle");
            _popups.PopupEntity(msg, uid, args.Actor, PopupType.MediumCaution);
            return;
        }

        var xform = Transform(uid);
        if (xform.Anchored)
        {
            _transform.Unanchor(uid, xform);
            _itemSlots.SetLock(uid, component.DiskSlot, true);
        }
        else
        {
            if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
                return;

            var worldPos = _transform.GetWorldPosition(xform);

            foreach (var tile in _map.GetTilesIntersecting(xform.GridUid.Value, grid, new Circle(worldPos, component.RequiredFloorRadius), false))
            {
                if (!_turf.IsSpace(tile))
                    continue;

                var msg = Loc.GetString("nuke-component-cant-anchor-floor");
                _popups.PopupEntity(msg, uid, args.Actor, PopupType.MediumCaution);

                return;
            }

            _transform.SetCoordinates(uid, xform, xform.Coordinates.SnapToGrid());
            _transform.AnchorEntity(uid, xform);
            _itemSlots.SetLock(uid, component.DiskSlot, false);
        }

        UpdateUserInterface(uid, component);
    }

    private void OnEnterButtonPressed(EntityUid uid, NukeComponent component, NukeKeypadEnterMessage args)
    {
        if (component.Status != NukeStatus.AWAIT_CODE)
            return;

        var curTime = _timing.CurTime;
        if (curTime < component.LastCodeEnteredAt + SharedNukeComponent.EnterCodeCooldown)
            return;

        component.LastCodeEnteredAt = curTime;

        UpdateStatus(uid, component);
        UpdateUserInterface(uid, component);
    }

    private void OnKeypadButtonPressed(EntityUid uid, NukeComponent component, NukeKeypadMessage args)
    {
        PlayNukeKeypadSound(uid, args.Value, component);

        if (component.Status != NukeStatus.AWAIT_CODE)
            return;

        if (component.EnteredCode.Length >= component.CodeLength)
            return;

        component.EnteredCode += args.Value.ToString();
        UpdateUserInterface(uid, component);
    }

    private void OnClearButtonPressed(EntityUid uid, NukeComponent component, NukeKeypadClearMessage args)
    {
        _audio.PlayPvs(component.KeypadPressSound, uid);

        if (component.Status != NukeStatus.AWAIT_CODE)
            return;

        component.EnteredCode = "";
        UpdateUserInterface(uid, component);
    }

    private void OnArmButtonPressed(EntityUid uid, NukeComponent component, NukeArmedMessage args)
    {
        var isOverride = GetDiskOverrideStatus(component.DiskSlot.Item);

        if (!component.DiskSlot.HasItem && !component.Converted) // Goobstation
            return;

        if (component.Status == NukeStatus.AWAIT_ARM && Transform(uid).Anchored)
            ArmBomb(uid, component);

        else
        {
            if (isOverride) // Goobstation
            {
                var msg = Loc.GetString("nuke-component-disarm-fail");
                _popups.PopupEntity(msg, uid, args.Actor, PopupType.MediumCaution);
                return;
            }

            DisarmBombDoAfter(uid, args.Actor, component);
        }
    }

    #endregion

    #region Doafter Events

    private void OnDoAfter(EntityUid uid, NukeComponent component, DoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        DisarmBomb(uid, component);
        GoobRaiseDisarmed(uid, args.User); // Goob

        var ev = new NukeDisarmSuccessEvent();
        RaiseLocalEvent(ev);

        args.Handled = true;
    }
    #endregion

    private void TickCooldown(EntityUid uid, float frameTime, NukeComponent? nuke = null)
    {
        if (!Resolve(uid, ref nuke))
            return;

        nuke.CooldownTime -= frameTime;
        if (nuke.CooldownTime <= 0)
        {
            nuke.CooldownTime = 0;
            nuke.Status = NukeStatus.AWAIT_ARM;
            UpdateStatus(uid, nuke);
        }

        UpdateUserInterface(uid, nuke);
    }

    private void TickTimer(EntityUid uid, float frameTime, NukeComponent? nuke = null)
    {
        if (!Resolve(uid, ref nuke))
            return;

        nuke.RemainingTime -= frameTime;

        if (nuke.RemainingTime <= _nukeSongLength + nuke.AlertSoundTime + NukeSongBuffer && !nuke.PlayedNukeSong && !ResolvedSoundSpecifier.IsNullOrEmpty(_selectedNukeSong))
        {
            _sound.DispatchStationEventMusic(uid, _selectedNukeSong, StationEventMusicType.Nuke);
            nuke.PlayedNukeSong = true;
        }

        if (nuke.RemainingTime <= nuke.AlertSoundTime && !nuke.PlayedAlertSound)
        {
            _sound.PlayGlobalOnStation(uid, _audio.ResolveSound(nuke.AlertSound), new AudioParams{Volume = -5f});
            _sound.StopStationEventMusic(uid, StationEventMusicType.Nuke);
            nuke.PlayedAlertSound = true;
            UpdateAppearance(uid, nuke);
        }

        if (nuke.RemainingTime <= 0)
        {
            nuke.RemainingTime = 0;
            ActivateBomb(uid, nuke);
        }

        else
            UpdateUserInterface(uid, nuke);
    }

    private void UpdateStatus(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var isOverride = GetDiskOverrideStatus(component.DiskSlot.Item); // Goobstation

        switch (component.Status)
        {
            case NukeStatus.AWAIT_DISK:
                if (component.DiskSlot.HasItem && !isOverride) // Goobstation
                    component.Status = NukeStatus.AWAIT_CODE;
                else if (component.DiskSlot.HasItem && isOverride) // Goobstation
                    component.Status = NukeStatus.AWAIT_ARM;
                break;
            case NukeStatus.AWAIT_CODE:
                if (!component.DiskSlot.HasItem && !component.Converted) // Goobstation
                {
                    component.Status = NukeStatus.AWAIT_DISK;
                    component.EnteredCode = "";
                    break;
                }

                if (component.EnteredCode == component.Code)
                {
                    component.Status = NukeStatus.AWAIT_ARM;
                    // Goobstation start - nuke ops siege rework
                    if (component.Converted)
                    {
                        component.RemainingTime = component.ConvertedTimer;
                    }
                    else
                    {
                        var modifier = CompOrNull<NukeDiskComponent>(component.DiskSlot.Item)?.TimeModifier ?? TimeSpan.Zero;
                        component.RemainingTime = MathF.Max(component.Timer + (float)modifier.TotalSeconds, component.MinimumTime);
                    }
                    // Goobstation end
                    _audio.PlayPvs(component.AccessGrantedSound, uid);
                }
                else
                {
                    component.EnteredCode = "";
                    _audio.PlayPvs(component.AccessDeniedSound, uid);
                }

                break;
            case NukeStatus.AWAIT_ARM:
                break;
            case NukeStatus.ARMED:
                // Goobstation
                if (!component.DiskSlot.HasItem && !component.Converted)
                {
                    DisarmBomb(uid, component);
                }

                break;
        }
    }

    private void UpdateUserInterface(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!_ui.HasUi(uid, NukeUiKey.Key))
            return;

        var anchored = Transform(uid).Anchored;

        // Goobstation
        var allowArm = (component.DiskSlot.HasItem || component.Converted) &&
                       (component.Status == NukeStatus.AWAIT_ARM ||
                        component.Status == NukeStatus.ARMED);

        var state = new NukeUiState
        {
            Status = component.Status,
            RemainingTime = (int) component.RemainingTime,
            DiskInserted = component.DiskSlot.HasItem,
            IsAnchored = anchored,
            AllowArm = allowArm,
            EnteredCodeLength = component.EnteredCode.Length,
            MaxCodeLength = component.CodeLength,
            CooldownTime = (int) component.CooldownTime,
            Converted = component.Converted, // Goobstation
        };

        _ui.SetUiState(uid, NukeUiKey.Key, state);
    }

    private void PlayNukeKeypadSound(EntityUid uid, int number, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var semitoneShift = number switch
        {
            1 => 0,
            2 => 2,
            3 => 3,
            4 => 4,
            5 => 5,
            6 => 6,
            7 => 7,
            8 => 9,
            9 => 10,
            0 => component.LastPlayedKeypadSemitones + 12,
            _ => 0,
        };

        component.LastPlayedKeypadSemitones = number == 0 ? component.LastPlayedKeypadSemitones : semitoneShift;

        var opts = component.KeypadPressSound.Params;
        opts = AudioHelpers.ShiftSemitone(opts, semitoneShift).AddVolume(-5f);
        _audio.PlayPvs(component.KeypadPressSound, uid, opts);
    }

    public string GenerateRandomNumberString(int length)
    {
        var ret = "";
        for (var i = 0; i < length; i++)
        {
            var c = (char) _random.Next('0', '9' + 1);
            ret += c;
        }

        return ret;
    }
    private bool GetDiskOverrideStatus(EntityUid? diskItem) // Goobstation
    {
        if (diskItem == null)
            return false;
        return TryComp<NukeDiskComponent>(diskItem, out var diskComp) && diskComp.Override;
    }

    #region Public API

    public void ArmBomb(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Status == NukeStatus.ARMED)
            return;

        var isOverride = GetDiskOverrideStatus(component.DiskSlot.Item); // Goobstation

        var nukeXform = Transform(uid);
        var stationUid = _station.GetStationInMap(nukeXform.MapID);
        if (stationUid != null && !isOverride)
            _alertLevel.SetLevel(stationUid.Value, component.AlertLevelOnActivate, true, true, true, true);
        else if (stationUid != null && isOverride)
            _alertLevel.SetLevel(stationUid.Value, component.AlertLevelOnOverride, true, true, true, true );

        var pos = _transform.GetMapCoordinates(uid, xform: nukeXform);
        var x = (int) pos.X;
        var y = (int) pos.Y;
        var posText = $"({x}, {y})";

        _selectedNukeSong = _audio.ResolveSound(component.ArmMusic);

        // Goobstation start
        var activeRules = _gameTicker.GetActiveGameRules();

        foreach (var rule in activeRules)
        {
            if (TryComp<NukeopsRuleComponent>(rule, out var nukeopsComp))
            {
                if (nukeopsComp.LocalePrefix == "honkops-")
                {
                    _selectedNukeSong = _audio.GetSound(component.HonkopsArmMusic);
                    break;
                }

                if (nukeopsComp.LocalePrefix == "loneop-")
                {
                    _selectedNukeSong = _audio.GetSound(component.LoneopArmMusic);
                    break;
                }
            }
        }
        // Goobstation end

        var announcement = Loc.GetString("nuke-component-announcement-armed",
            ("time", (int) component.RemainingTime),
            ("location", FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString((uid, nukeXform)))));
        var sender = Loc.GetString("nuke-component-announcement-sender");
        _chatSystem.DispatchStationAnnouncement(stationUid ?? uid, announcement, sender, false, null, Color.Red);

        _sound.PlayGlobalOnStation(uid, _audio.ResolveSound(component.ArmSound));
        _nukeSongLength = (float) _audio.GetAudioLength(_selectedNukeSong).TotalSeconds;

        _pointLight.SetEnabled(uid, true);
        _navMap.SetBeaconEnabled(uid, true);

        _itemSlots.SetLock(uid, component.DiskSlot, !component.Converted); // Goobstation
        if (!nukeXform.Anchored)
        {
            _transform.AnchorEntity(uid, nukeXform);
        }

        component.Status = NukeStatus.ARMED;
        UpdateUserInterface(uid, component);
        UpdateAppearance(uid, component);
    }

    public void DisarmBomb(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var isOverride = GetDiskOverrideStatus(component.DiskSlot.Item); // Goobstation

        if (component.Status != NukeStatus.ARMED || isOverride ) // Goobstation - Extra Safeguard
            return;

        var stationUid = _station.GetOwningStation(uid);
        if (stationUid != null)
            _alertLevel.SetLevel(stationUid.Value, component.AlertLevelOnDeactivate, true, true, true);

        var announcement = Loc.GetString("nuke-component-announcement-unarmed");
        var sender = Loc.GetString("nuke-component-announcement-sender");
        _chatSystem.DispatchStationAnnouncement(uid, announcement, sender, false);

        component.PlayedNukeSong = false;
        _sound.PlayGlobalOnStation(uid, _audio.ResolveSound(component.DisarmSound));
        _sound.StopStationEventMusic(uid, StationEventMusicType.Nuke);

        component.RemainingTime = Math.Max(component.RemainingTime, component.MinimumTime);

        component.PlayedAlertSound = false;
        component.AlertAudioStream = _audio.Stop(component.AlertAudioStream);

        _pointLight.SetEnabled(uid, false);
        _navMap.SetBeaconEnabled(uid, false);

        _itemSlots.SetLock(uid, component.DiskSlot, false);
        component.Status = NukeStatus.COOLDOWN;
        component.CooldownTime = component.Cooldown;

        UpdateUserInterface(uid, component);
        UpdateAppearance(uid, component);
    }

    public void ToggleBomb(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Status == NukeStatus.ARMED)
            DisarmBomb(uid, component);
        else
            ArmBomb(uid, component);
    }

    public void ActivateBomb(EntityUid uid, NukeComponent? component = null,
        TransformComponent? transform = null)
    {
        if (!Resolve(uid, ref component, ref transform))
            return;

        if (component.Exploded)
            return;

        component.Exploded = true;

        _explosions.QueueExplosion(uid,
            component.ExplosionType,
            component.TotalIntensity,
            component.IntensitySlope,
            component.MaxIntensity);

        RaiseLocalEvent(new NukeExplodedEvent()
        {
            OwningStation = transform.GridUid,
        });

        _sound.StopStationEventMusic(uid, StationEventMusicType.Nuke);
        Del(uid);
    }

    public void SetRemainingTime(EntityUid uid, float timer, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.RemainingTime = timer;
        UpdateUserInterface(uid, component);
    }

    // Goobstation start - nuke ops siege rework
    public void SetNukeCode(EntityUid uid, string code, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Code = code;
    }

    public void ConvertBomb(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Converted || component.Status == NukeStatus.ARMED)
            return;

        component.Converted = true;

        if (component.Status == NukeStatus.AWAIT_DISK)
            component.Status = NukeStatus.AWAIT_CODE;

        UpdateUserInterface(uid, component);
    }

    public void SilentlyDeactivate(EntityUid uid, NukeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.Status != NukeStatus.ARMED)
            return;

        component.PlayedNukeSong = false;
        component.PlayedAlertSound = false;
        component.AlertAudioStream = _audio.Stop(component.AlertAudioStream);
        _sound.StopStationEventMusic(uid, StationEventMusicType.Nuke);
        _pointLight.SetEnabled(uid, false);
        _navMap.SetBeaconEnabled(uid, false);
        component.Status = NukeStatus.COOLDOWN;
        component.CooldownTime = component.Cooldown;
    }
    // Goobstation end

    #endregion

    private void DisarmBombDoAfter(EntityUid uid, EntityUid user, NukeComponent nuke)
    {
        var doAfter = new DoAfterArgs(EntityManager, user, nuke.DisarmDoAfterLength, new NukeDisarmDoAfterEvent(), uid, target: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = true,
            MultiplyDelay = false, // Goobstation
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        _popups.PopupEntity(Loc.GetString("nuke-component-doafter-warning"),
            user,
            user,
            PopupType.LargeCaution);
    }

    private void UpdateAppearance(EntityUid uid, NukeComponent nuke)
    {
        var xform = Transform(uid);

        _appearance.SetData(uid, NukeVisuals.Deployed, xform.Anchored);

        NukeVisualState state;
        if (nuke.PlayedAlertSound)
            state = NukeVisualState.YoureFucked;
        else if (nuke.Status == NukeStatus.ARMED)
            state = NukeVisualState.Armed;
        else
            state = NukeVisualState.Idle;

        _appearance.SetData(uid, NukeVisuals.State, state);
    }

    private void OnExaminedEvent(EntityUid uid, NukeComponent component, ExaminedEvent args)
    {
        if (component.PlayedAlertSound)
            args.PushMarkup(Loc.GetString("nuke-examine-exploding"));
        else if (component.Status == NukeStatus.ARMED)
            args.PushMarkup(Loc.GetString("nuke-examine-armed"));

        if (Transform(uid).Anchored)
            args.PushMarkup(Loc.GetString("examinable-anchored"));
        else
            args.PushMarkup(Loc.GetString("examinable-unanchored"));
    }
}

public sealed class NukeExplodedEvent : EntityEventArgs
{
    public EntityUid? OwningStation;
}

public sealed class NukeDisarmSuccessEvent : EntityEventArgs
{

}

