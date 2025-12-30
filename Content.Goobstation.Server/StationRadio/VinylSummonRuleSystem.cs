using Content.Goobstation.Shared.StationRadio.Components;
using Content.Goobstation.Shared.StationRadio.Events;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Shared.Communications;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DeviceLinking;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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
        // Check if the inserted entity has the summon rule component
        if (!TryComp<VinylSummonRuleComponent>(args.Vinyl, out _))
            return;

        // Check if the vinyl has a song
        if (!TryComp<VinylComponent>(args.Vinyl, out var vinylComp)
            || vinylComp.Song == null)
            return;

        // Check if vinyl player is on a station
        if (_stationSystem.GetOwningStation(uid) == null)
            return;

        // Check if vinyl player is powered
        if (!_power.IsPowered(uid))
            return;

        // Check if vinyl player is connected to the radio system
        if (!CheckForRadioRig(uid))
            return;

        // Get the audio length
        var resolved = _audio.ResolveSound(vinylComp.Song);
        var audioLength = _audio.GetAudioLength(resolved);
        var endTime = _timing.CurTime + audioLength;

        // Track this vinyl with its player
        _trackingVinyls[args.Vinyl] = new TrackingData(uid, endTime);
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
        var toRemove = new List<EntityUid>();

        foreach (var (vinylUid, data) in _trackingVinyls)
        {
            // Check if the vinyl still exists
            if (!Exists(vinylUid))
            {
                toRemove.Add(vinylUid);
                continue;
            }

            // Check if the vinyl player still exists
            if (!Exists(data.VinylPlayerUid))
            {
                toRemove.Add(vinylUid);
                continue;
            }

            // Check if vinyl player is still on a station
            if (_stationSystem.GetOwningStation(data.VinylPlayerUid) == null)
            {
                toRemove.Add(vinylUid);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if vinyl player is still powered
            if (!_power.IsPowered(data.VinylPlayerUid))
            {
                toRemove.Add(vinylUid);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if vinyl player is still connected to the radio system
            if (!CheckForRadioRig(data.VinylPlayerUid))
            {
                toRemove.Add(vinylUid);
                EjectVinyl(data.VinylPlayerUid, vinylUid);
                continue;
            }

            // Check if playback has finished
            if (currentTime >= data.EndTime)
            {
                HandleVinylFinished(vinylUid);
                toRemove.Add(vinylUid);
            }
        }

        // Clean up finished tracking
        foreach (var uid in toRemove)
            _trackingVinyls.Remove(uid);
    }

    private void EjectVinyl(EntityUid playerUid, EntityUid vinylUid)
    {
        if (!Exists(vinylUid) || !Exists(playerUid))
            return;

        if (!TryComp<ItemSlotsComponent>(playerUid, out var itemSlots))
            return;

        // Find the slot containing the vinyl
        foreach (var (slotId, slot) in itemSlots.Slots)
        {
            if (slot.Item == vinylUid)
            {
                _itemSlots.TryEject(playerUid, slot, null, out _);
                return;
            }
        }
    }

    private void HandleVinylFinished(EntityUid vinylUid)
    {
        if (!TryComp<VinylSummonRuleComponent>(vinylUid, out var summonComp))
            return;

        // Resolve the game rule ID
        var ruleId = ResolveGameRule(summonComp.GameRule);
        if (ruleId != null)
            _gameTicker.StartGameRule(ruleId, out _);

        // Destroy the vinyl and spawn ash
        DestroyVinylAndSpawnAsh(vinylUid, summonComp);
    }

    private string? ResolveGameRule(string gameRuleIdentifier)
    {
        // Check if it's a weighted random pool
        if (_prototypeManager.TryIndex<WeightedRandomPrototype>(gameRuleIdentifier, out var weightedPool))
        {
            // Pick a random threat ID from the weighted pool
            var threatId = weightedPool.Pick(_random);

            // Look up the threat prototype to get the actual game rule ID
            if (_prototypeManager.TryIndex<NinjaHackingThreatPrototype>(threatId, out var threat))
                return threat.Rule;

            return null;
        }

        // Assume it's a direct game rule entity ID
        return gameRuleIdentifier;
    }

    private void DestroyVinylAndSpawnAsh(EntityUid vinylUid, VinylSummonRuleComponent summonComp)
    {
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

    private bool CheckForRadioRig(EntityUid uid)
    {
        if (TryComp<DeviceLinkSourceComponent>(uid, out var source))
            foreach (var linked in source.LinkedPorts.Keys)
                if (HasComp<RadioRigComponent>(linked) && CheckForRadioServer(linked))
                    return true;
        return false;
    }

    private bool CheckForRadioServer(EntityUid uid)
    {
        if (TryComp<DeviceLinkSinkComponent>(uid, out var source))
            foreach (var linked in source.LinkedSources)
                if (HasComp<StationRadioServerComponent>(linked))
                    return true;
        return false;
    }
}
