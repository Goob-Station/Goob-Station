using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Communications;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Server.Radio.Components;
using Content.Server.Chat.Systems;

namespace Content.Goobstation.Server.StationRadio;

/// <summary>
/// System that handles spawning game rules when vinyl disks finish playing.
/// </summary>
public sealed class VinylSummonRuleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popups = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    private record struct TrackingData(EntityUid VinylPlayerUid, TimeSpan EndTime);
    private readonly Dictionary<EntityUid, TrackingData> _trackingVinyls = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VinylPlayerComponent, VinylInsertedEvent>(OnVinylInserted);
        SubscribeLocalEvent<VinylPlayerComponent, VinylRemovedEvent>(OnVinylRemoved);
    }

    private void OnVinylInserted(EntityUid uid, VinylPlayerComponent player, ref VinylInsertedEvent args)
    {
        var playerUid = uid;
        var vinylUid = args.Vinyl;

        // Check if the inserted entity has the summon rule component / A song
        if (!TryComp<VinylSummonRuleComponent>(vinylUid, out _)
            || !TryComp<VinylComponent>(vinylUid, out var vinylComp)
            || vinylComp.Song == null)
            return;

        void QueueSafeEject()
        {
            Timer.Spawn(0, () => EjectVinyl(playerUid, vinylUid));
        }

        // Check if vinyl player is on a station
        if (_stationSystem.GetOwningStation(playerUid) == null)
        {
            _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-station"), playerUid, null, PopupType.Medium);
            QueueSafeEject();
            return;
        }

        // Check if vinyl player is powered
        if (!_power.IsPowered(playerUid))
        {
            _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-power"), playerUid, null, PopupType.Medium);
            QueueSafeEject();
            return;
        }

        // Check if vinyl player is connected to the radio system
        if (!CheckForRadioConnection(playerUid))
        {
            _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-radio-connection"), playerUid, null, PopupType.Medium);
            QueueSafeEject();
            return;
        }

        // Get the audio length
        var resolved = _audio.ResolveSound(vinylComp.Song);
        var audioLength = _audio.GetAudioLength(resolved);
        var endTime = _timing.CurTime + audioLength;

        // Track this vinyl with its player
        _trackingVinyls[vinylUid] = new TrackingData(playerUid, endTime);
    }

    private void OnVinylRemoved(EntityUid uid, VinylPlayerComponent player, ref VinylRemovedEvent args)
    {
        // Stop tracking if the vinyl is removed
        _trackingVinyls.Remove(args.Vinyl);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var currentTime = _timing.CurTime;

        foreach (var (vinylUid, data) in _trackingVinyls.ToList())
        {
            // Check if the vinyl still exists
            if (!Exists(vinylUid)
                || !Exists(data.VinylPlayerUid))
            {
                _trackingVinyls.Remove(vinylUid);
                continue;
            }

            // Check if vinyl player is still on a station
            if (_stationSystem.GetOwningStation(data.VinylPlayerUid) == null)
            {
                _trackingVinyls.Remove(vinylUid);
                _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-station"), data.VinylPlayerUid, null, PopupType.Medium);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if vinyl player is still powered
            if (!_power.IsPowered(data.VinylPlayerUid))
            {
                _trackingVinyls.Remove(vinylUid);
                _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-power"), data.VinylPlayerUid, null, PopupType.Medium);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if vinyl player is still connected to the radio system
            if (!CheckForRadioConnection(data.VinylPlayerUid))
            {
                _trackingVinyls.Remove(vinylUid);
                _popups.PopupPredicted(Loc.GetString("vinyl-popout-no-radio-connection"), data.VinylPlayerUid, null, PopupType.Medium);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if playback has finished
            if (currentTime >= data.EndTime)
            {
                HandleVinylFinished(vinylUid);
                _trackingVinyls.Remove(vinylUid);
            }
        }
    }

    private void EjectVinyl(EntityUid playerUid, EntityUid vinylUid)
    {
        if (!Exists(vinylUid)
            || !Exists(playerUid)
            || !TryComp<ItemSlotsComponent>(playerUid, out var itemSlots))
            return;

        // Find the slot containing the vinyl
        foreach (var (_, slot) in itemSlots.Slots)
            if (slot.Item == vinylUid)
            {
                _itemSlots.TryEject(playerUid, slot, null, out _);
                return;
            }
    }

    private void HandleVinylFinished(EntityUid vinylUid)
    {
        if (!TryComp<VinylSummonRuleComponent>(vinylUid, out var summonComp))
            return;

        // Resolve the game rule ID and get the threat prototype if available
        var ruleId = ResolveGameRule(summonComp.GameRule, out var threat);

        if (ruleId != null)
        {
            _gameTicker.StartGameRule(ruleId, out _);

            // If we have a threat prototype with an announcement, send it
            if (threat != null)
                _chat.DispatchGlobalAnnouncement(Loc.GetString(threat.Announcement), playSound: true, colorOverride: Color.Red);
        }

        var vinylXform = Transform(vinylUid);
        var vinylCoords = vinylXform.Coordinates;

        // Remove from container
        if (_containers.TryGetContainingContainer((vinylUid, vinylXform, null), out var container))
            _containers.Remove(vinylUid, container);

        // Play sound effect
        _audio.PlayPvs(summonComp.BurnSound, vinylCoords, AudioParams.Default.WithVolume(-5f));

        // Spawn ash at the vinyl's location
        Spawn("Ash", vinylCoords);

        // Delete the vinyl
        QueueDel(vinylUid);
    }

    private string? ResolveGameRule(string gameRuleIdentifier, out NinjaHackingThreatPrototype? threat)
    {
        threat = null;

        // Check if it's a weighted random pool
        if (_prototypeManager.TryIndex<WeightedRandomPrototype>(gameRuleIdentifier, out var weightedPool))
        {
            // Pick a random threat ID from the weighted pool
            var threatId = weightedPool.Pick(_random);

            // Look up the threat prototype to get the actual game rule ID
            if (_prototypeManager.TryIndex<NinjaHackingThreatPrototype>(threatId, out threat))
                return threat.Rule;

            return null;
        }

        // Assume it's a direct game rule entity ID
        return gameRuleIdentifier;
    }

    private bool CheckForRadioConnection(EntityUid uid)
    {
        if (!TryComp<DeviceLinkSourceComponent>(uid, out var source))
            return false;

        foreach (var linkedRig in source.LinkedPorts.Keys)
        {
            // Check if the radio rig is connected.
            if (!HasComp<RadioRigComponent>(linkedRig)
                || !TryComp<DeviceLinkSinkComponent>(linkedRig, out var sink))
                continue;

            // Check if the radio server is connected.
            foreach (var linkedServer in sink.LinkedSources)
            {
                if (!TryComp<StationRadioServerComponent>(linkedServer, out var _)
                    || !_power.IsPowered(linkedServer))
                    continue;

                return true;
            }
        }

        return false;
    }
}
