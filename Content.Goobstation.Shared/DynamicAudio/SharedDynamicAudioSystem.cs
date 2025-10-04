using Content.Goobstation.Shared.DynamicAudio.Effects;
using Content.Shared.GameTicking;
using Content.Shared.Maps;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Physics;
using Robust.Shared.Map;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Content.Goobstation.Shared.DynamicAudio;

public sealed class SharedDynamicAudioSystem : EntitySystem
{
    private Dictionary<ProtoId<AudioPresetPrototype>, EntityUid> _presets = new();
    private Dictionary<int, string> _areaPresets = new Dictionary<int, string> // sort it or will be broken
    {
        { 10, "PaddedCell" },
        { 70, "Generic" }, // no echo / normal
        { 80, "SpaceStationMediumRoom" },
        { 100, "SpaceStationLargeRoom" },
        { 200, "SpaceStationHall" }
    };

    private string _defaultPreset = "LivingRoom";
    private string _inSpacePreset = "InSpace";
    private string _onPlanetPreset = "Forest";

    private int _maxAreaScanRadius = 8; // prefer to set value of pvs divided by 2
    private int _maxTilesScanCount = 200;

    private int _soundMuffleInSpace = -10;

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookUp = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(_ => _presets.Clear());
    }

    /// <summary>
    /// Applies sound effects by it's environment.
    /// </summary>
    public void ApplyAudioEffect(Entity<AudioComponent> audio, EntityUid? source = null)
    {
        if (!_playerManager.LocalEntity.HasValue)
            return;

        var preset = GetAreaPrototypePreset(audio, source);

        if (!_presets.TryGetValue(preset, out var audioAux) && !TryCreateAudioEffect(preset, out audioAux))
            return;

        if (_net.IsServer)
            Timer.Spawn(TimeSpan.FromTicks(10L), () => _audio.SetAuxiliary(audio, audio, audioAux));
        else
            _audio.SetAuxiliary(audio, audio, audioAux);
    }

    /// <summary>
    /// Calculates sound area by it's effects components, local player position etc.
    /// </summary>
    private string GetAreaPrototypePreset(Entity<AudioComponent> audio, EntityUid? source = null)
    {
        var soundPosition = source is null ? _xform.GetWorldPosition(audio) : _xform.GetWorldPosition(source.Value);
        var soundTransform = source is null ? Transform(audio) : Transform(source.Value);

        if (soundPosition == Vector2.Zero && _playerManager.LocalEntity.HasValue)
        {
            soundPosition = _xform.GetWorldPosition(_playerManager.LocalEntity.Value);
            soundTransform = Transform(_playerManager.LocalEntity.Value);
        }

        // checks if sound or player in space
        if (soundTransform.GridUid is null || HasComp<InBarotraumaAudioEffectComponent>(soundTransform.Owner)
            || _playerManager.LocalEntity.HasValue && HasComp<InBarotraumaAudioEffectComponent>(_playerManager.LocalEntity))
        {
            _audio.SetVolume(audio, _soundMuffleInSpace);
            return _inSpacePreset;
        }

        // checks if sound on planet
        if (HasComp<BiomeComponent>(soundTransform.GridUid))
            return _onPlanetPreset;

        if (!TryComp<MapGridComponent>(soundTransform.GridUid, out var mapGrid))
            return _defaultPreset;

        int estimatedArea = CountTilesInRoom(_map.TileIndicesFor(soundTransform.GridUid.Value, mapGrid, _xform.GetMapCoordinates(soundTransform)), (soundTransform.GridUid.Value, mapGrid));

        foreach (var areaPreset in _areaPresets)
            if (estimatedArea <= areaPreset.Key)
                return areaPreset.Value;

        return _defaultPreset;
    }

    /// <summary>
    /// Calculates room area tile by tile.
    /// </summary>
    private int CountTilesInRoom(Vector2i startTile, Entity<MapGridComponent> grid)
    {
        var visited = new HashSet<Vector2i>();
        var queue = new Queue<Vector2i>();

        queue.Enqueue(startTile);
        visited.Add(startTile);

        var directions = new Vector2i[] { new(0, 1), new(1, 0), new(0, -1), new(-1, 0) };

        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();

            var distance = Math.Abs(currentTile.X - startTile.X) +
                          Math.Abs(currentTile.Y - startTile.Y);

            if (distance > _maxAreaScanRadius) // must be restricted by radius or your CPU gets fuck
                continue;

            if (visited.Count > _maxTilesScanCount) // and also by maximum of tiles
                break;

            foreach (var direction in directions)
            {
                var neighborTile = currentTile + direction;

                if (visited.Contains(neighborTile))
                    continue;

                if (IsValidTile(neighborTile, grid))
                {
                    visited.Add(neighborTile);
                    queue.Enqueue(neighborTile);
                }
            }
        }

        return visited.Count;
    }

    /// <summary>
    /// Validates tile for being space or solid structure in it like wall that can block sound.
    /// </summary>
    private bool IsValidTile(Vector2i tilePos, Entity<MapGridComponent> grid)
    {
        if (!_map.TryGetTileRef(grid.Owner, grid.Comp, tilePos, out var tile))
            return false;

        if (_turf.IsSpace(tile))
            return false;

        var loc = _map.GridTileToWorld(grid.Owner, grid.Comp, tilePos);
        foreach (var ent in _lookUp.GetEntitiesInRange(loc, 0.01f))
            if (TryComp<PhysicsComponent>(ent, out var phys) &&
                phys.BodyType == BodyType.Static &&
                phys.Hard &&
                (phys.CollisionLayer & (int) (CollisionGroup.Impassable | CollisionGroup.HighImpassable)) != 0)
            {
                return false;
            }

        return true;
    }

    /// <summary>
    /// Tries to get effect from dictionary or create and save it.
    /// </summary>
    private bool TryCreateAudioEffect(ProtoId<AudioPresetPrototype> preset, [NotNullWhen(true)] out EntityUid auxUid)
    {
        auxUid = default;

        if (!_prototype.TryIndex<AudioPresetPrototype>(preset, out var presetPrototype))
            return false;

        var effectEntity = _audio.CreateEffect();
        var auxEntity = _audio.CreateAuxiliary();

        _audio.SetEffectPreset(effectEntity.Entity, effectEntity.Component, presetPrototype);
        _audio.SetEffect(auxEntity.Entity, auxEntity.Component, effectEntity.Entity);

        if (Exists(auxEntity.Entity))
        {
            auxUid = auxEntity.Entity;
            _presets.Add(preset, auxUid);
            return true;
        }

        return false;
    }
}
