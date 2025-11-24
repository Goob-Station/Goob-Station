using System.Numerics;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Movement.Components;
using Content.Shared.Physics;
using Content.Shared.StationAi;
using Content.Trauma.Common.CCVar;
using Content.Trauma.Shared.AudioMuffle;
using Robust.Client.Audio;
using Robust.Client.GameObjects;
using Robust.Client.GameStates;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Trauma.Client.AudioMuffle;

public sealed partial class AudioMuffleSystem : SharedAudioMuffleSystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PhysicsSystem _physics = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IClientGameStateManager _stateMan = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private static EntityQuery<GhostComponent> _ghostQuery;
    private static EntityQuery<SpectralComponent> _spectralQuery;
    private static EntityQuery<RelayInputMoverComponent> _relayedQuery;
    private static EntityQuery<AiEyeComponent> _aiEyeQuery;
    private static EntityQuery<AudioComponent> _audioQuery;
    private static EntityQuery<SoundBlockerComponent> _blockerQuery;

    // Tile indices -> blocker entities
    [ViewVariables]
    public readonly Dictionary<Vector2i, HashSet<Entity<SoundBlockerComponent>>> ReverseBlockerIndicesDict = new();

    // Tile indices -> list of audio entities
    [ViewVariables]
    public readonly Dictionary<Vector2i, HashSet<Entity<AudioComponent, AudioMuffleComponent>>> ReverseAudioPosDict =
        new();

    // Tile indices -> data
    [ViewVariables]
    public readonly Dictionary<Vector2i, MuffleTileData> TileDataDict = new();

    [ViewVariables]
    public Entity<MapGridComponent>? PlayerGrid;

    [ViewVariables]
    public Vector2i? OldPlayerTile;

    private readonly HashSet<Entity<StationAiVisionComponent>> _nearestVisionEntities = new();

    private readonly List<Entity<AudioComponent, AudioMuffleComponent>> _audioToRemove = new();

    private readonly List<Entity<SoundBlockerComponent>> _blockersToRemove = new();

    private const int AudioRange = (int) SharedAudioSystem.DefaultSoundRange;

    private const int PathfindingRange = AudioRange + 3;

    private const float ShortAudioLength = 4f;

    private bool _raycastEnabled = true;

    private bool _pathfindingEnabled = true;

    private bool _shouldResetRaycastFully;

    [Flags]
    private enum AudioProcessBehavior : byte
    {
        None = 0,
        Reset,
        Recalculate,
    }

    public override void Initialize()
    {
        base.Initialize();

        _ghostQuery = GetEntityQuery<GhostComponent>();
        _spectralQuery = GetEntityQuery<SpectralComponent>();
        _relayedQuery = GetEntityQuery<RelayInputMoverComponent>();
        _aiEyeQuery = GetEntityQuery<AiEyeComponent>();
        _audioQuery = GetEntityQuery<AudioComponent>();
        _blockerQuery = GetEntityQuery<SoundBlockerComponent>();

        _xform.OnGlobalMoveEvent += OnMove;
        _stateMan.GameStateApplied += OnGameStateApplied;

        UpdatesOutsidePrediction = true;

        SubscribeLocalEvent<LocalPlayerDetachedEvent>(OnLocalPlayerDetached);
        SubscribeLocalEvent<LocalPlayerAttachedEvent>(OnLocalPlayerAttached);

        SubscribeLocalEvent<SoundBlockerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<SoundBlockerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SoundBlockerComponent, AfterAutoHandleStateEvent>(OnBlockerState);

        SubscribeLocalEvent<AudioComponent, ComponentInit>(OnInit);

        SubscribeLocalEvent<AudioMuffleComponent, ComponentShutdown>(OnMuffleShutdown);

        SubscribeNetworkEvent<RoundRestartCleanupEvent>(OnRestart);

        Subs.CVar(_cfg, TraumaCVars.AudioMuffleRaycast, value => _raycastEnabled = value, true);
        Subs.CVar(_cfg, TraumaCVars.AudioMufflePathfinding, value => _pathfindingEnabled = value, true);
    }

    private void OnRestart(RoundRestartCleanupEvent ev)
    {
        PlayerGrid = null;
        OldPlayerTile = null;
        ClearDicts();
    }

    public override void Shutdown()
    {
        base.Shutdown();

        PlayerGrid = null;
        OldPlayerTile = null;
        ClearDicts();

        _xform.OnGlobalMoveEvent -= OnMove;
        _stateMan.GameStateApplied -= OnGameStateApplied;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        EntityUid? player = null;

        var raycastBehavior = AudioProcessBehavior.Reset;
        if (_shouldResetRaycastFully)
        {
            raycastBehavior |= AudioProcessBehavior.Recalculate;
            player = ResolvePlayer();
        }
        _shouldResetRaycastFully = false;

        ReCalculateAllAudio(player, raycastBehavior, AudioProcessBehavior.Reset);
    }

    private void OnInit(Entity<AudioComponent> ent, ref ComponentInit args)
    {
        if (!CanMuffle(ent.Comp))
            return;

        if (ResolvePlayer() is not { } player)
            return;

        var muffle = EnsureComp<AudioMuffleComponent>(ent);
        muffle.OriginalVolume = ent.Comp.Params.Volume;
        ReCalculateAudioMuffle(player, (ent, ent.Comp, muffle), _xform.GetMapCoordinates(ent), false);
    }

    private void OnMuffleShutdown(Entity<AudioMuffleComponent> ent, ref ComponentShutdown args)
    {
        RemoveAudioMuffle(ent);
    }

    private void RemoveAudioMuffle(Entity<AudioMuffleComponent> ent)
    {
        var audioComp = _audioQuery.Comp(ent);
        if (ent.Comp.Indices is { } indices)
        {
            if (ReverseAudioPosDict.TryGetValue(indices, out var set))
            {
                set.Remove((ent, audioComp, ent.Comp));
                if (set.Count == 0)
                    ReverseAudioPosDict.Remove(indices);
            }
        }
    }


    private void ResetImmediate(EntityUid player)
    {
        ClearDicts();

        if (!_pathfindingEnabled && !_raycastEnabled)
            return;

        ResetAllBlockers(player);
        ReCalculateAllAudio(player);
    }

    private void ReCalculateAllAudio(EntityUid? player,
        AudioProcessBehavior raycastBehavior = AudioProcessBehavior.Recalculate,
        AudioProcessBehavior pathfindingBehavior = AudioProcessBehavior.Recalculate)
    {
        if (!_pathfindingEnabled && !_raycastEnabled ||
            raycastBehavior == AudioProcessBehavior.None && pathfindingBehavior == AudioProcessBehavior.None)
            return;

        var query = EntityQueryEnumerator<AudioMuffleComponent, AudioComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var muffle, out var audio, out var xform))
        {
            var hasIndices = muffle.Indices != null;
            var behavior = hasIndices ? pathfindingBehavior : raycastBehavior;
            var recalc = (behavior & AudioProcessBehavior.Recalculate) != 0x0;
            var reset = (behavior & AudioProcessBehavior.Reset) != 0x0;

            if (recalc && player != null)
                ReCalculateAudioMuffle(player.Value, (uid, audio), _xform.GetMapCoordinates(uid, xform), reset, false);
            else if (reset)
                ResetAudioMuffle((uid, audio, muffle));
        }
    }

    private void ResetAllBlockers(EntityUid player)
    {
        if (!_pathfindingEnabled && !_raycastEnabled)
            return;

        var query = EntityQueryEnumerator<SoundBlockerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var blocker, out var xform))
        {
            ResetBlockerMuffle(player, (uid, xform, blocker));
        }
    }

    private void OnBlockerState(Entity<SoundBlockerComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        ent.Comp.CachedBlockerCost = null;

        if (ResolvePlayer() is not { } player)
            return;

        ResetBlockerMuffle(player, (ent, null, ent));
    }

    private void OnStartup(Entity<SoundBlockerComponent> ent, ref ComponentStartup args)
    {
        if (ResolvePlayer() is not { } player)
            return;

        ResetBlockerMuffle(player, (ent, null, ent));
        _shouldResetRaycastFully = true;
    }

    private void OnShutdown(Entity<SoundBlockerComponent> ent, ref ComponentShutdown args)
    {
        RemoveBlocker(ent);
    }

    private void OnLocalPlayerAttached(LocalPlayerAttachedEvent ev)
    {
        ResetImmediate(ev.Entity);

        if (!_pathfindingEnabled)
            return;

        var pos = _xform.GetMapCoordinates(ev.Entity);
        if (ResolvePlayerGrid(pos) is not { } grid)
            return;

        var tile = _map.TileIndicesFor(grid, pos);

        if (!_map.CollidesWithGrid(grid, grid, tile))
            return;

        Expand(tile);
    }

    private void OnLocalPlayerDetached(LocalPlayerDetachedEvent ev)
    {
        TileDataDict.Clear();
    }

    private void OnMove(ref MoveEvent ev)
    {
        if (!_raycastEnabled && !_pathfindingEnabled)
            return;

        if (ev.OldPosition == ev.NewPosition)
            return;

        if (ResolvePlayer() is not { } player)
            return;

        var uid = ev.Entity.Owner;

        if (HasComp<MapGridComponent>(uid))
            return;

        var oldMap = ev.OldPosition.IsValid(EntityManager)
            ? _xform.ToMapCoordinates(ev.OldPosition)
            : MapCoordinates.Nullspace;
        var newMap = ev.NewPosition.IsValid(EntityManager)
            ? _xform.ToMapCoordinates(ev.NewPosition)
            : MapCoordinates.Nullspace;

        if (oldMap == MapCoordinates.Nullspace && newMap == MapCoordinates.Nullspace)
            return;

        ProcessEntityMove(player, uid, oldMap, newMap);

        var childEnumerator = ev.Entity.Comp1.ChildEnumerator;
        while (childEnumerator.MoveNext(out var child))
        {
            ProcessEntityMove(player, child, oldMap, newMap);
        }
    }

    private void ProcessEntityMove(EntityUid player,
        EntityUid uid,
        MapCoordinates oldPosition,
        MapCoordinates newPosition)
    {
        // ResolvePlayer returns "fake" player (ai vision entity) if local entity is ai eye
        if (_relayedQuery.TryComp(_player.LocalEntity, out var relay) &&
            uid == relay.RelayEntity && player != relay.RelayEntity)
        {
            PlayerMoved(player, MapCoordinates.Nullspace, _xform.GetMapCoordinates(player));
            return;
        }

        if (uid == player)
        {
            PlayerMoved(player, oldPosition, newPosition);
            return;
        }

        if (_blockerQuery.TryComp(uid, out var blocker))
        {
            ResetBlockerMuffle(player, (uid, null, blocker), oldPosition, newPosition);
            _shouldResetRaycastFully = true;
        }

        if (_audioQuery.TryComp(uid, out var audio))
            ReCalculateAudioMuffle(player, (uid, audio), newPosition, reset: false);
    }

    private void OnGameStateApplied(GameStateAppliedArgs args)
    {
        if (ResolvePlayer() is not { } player)
            return;

        foreach (var states in args.AppliedState.EntityStates.Value)
        {
            if (!TryGetEntity(states.NetEntity, out var ent))
                continue;

            if (!_audioQuery.TryComp(ent.Value, out var audioComp))
                continue;

            if (!CanMuffle(audioComp))
                continue;

            var muffle = EnsureComp<AudioMuffleComponent>(ent.Value);

            float? volume = null;

            foreach (var change in states.ComponentChanges.Value)
            {
                if (change.State is not AudioComponent.AudioComponent_AutoState state)
                    continue;

                volume = state.Params.Volume;
                break;
            }

            if (volume != null && !float.IsInfinity(volume.Value))
                muffle.OriginalVolume = volume.Value;
            else if (muffle.OriginalVolume == null)
            {
                if (float.IsInfinity(audioComp.Params.Volume))
                    return;

                muffle.OriginalVolume = audioComp.Params.Volume;
            }

            ReCalculateAudioMuffle(player, (ent.Value, audioComp, muffle), _xform.GetMapCoordinates(ent.Value));
        }
    }

    private void ClearDicts()
    {
        TileDataDict.Clear();
        ReverseAudioPosDict.Clear();
        ReverseBlockerIndicesDict.Clear();
    }

    public EntityUid? ResolvePlayer()
    {
        if (_player.LocalEntity is not { } player)
            return null;

        if (_relayedQuery.TryComp(player, out var relay) && _aiEyeQuery.HasComp(relay.RelayEntity))
        {
            if (FindNearestAiVisionEntity(relay.RelayEntity) is { } entity)
                return entity;

            return relay.RelayEntity;
        }

        if (_ghostQuery.HasComp(player) || _spectralQuery.HasComp(player))
            return null;

        return player;
    }

    public EntityUid? FindNearestAiVisionEntity(EntityUid player)
    {
        var coords = _xform.GetMapCoordinates(player);
        _nearestVisionEntities.Clear();
        _lookup.GetEntitiesInRange(coords, AudioRange, _nearestVisionEntities);
        EntityUid? result = null;
        var distance = float.MaxValue;
        foreach (var (uid, vision) in _nearestVisionEntities)
        {
            if (!vision.Enabled)
                continue;

            var dist = (coords.Position - _xform.GetMapCoordinates(uid).Position).Length();

            if (result != null && dist >= distance)
                continue;

            result = uid;
            distance = dist;
        }

        _nearestVisionEntities.Clear();
        return result;
    }

    public Entity<MapGridComponent>? ResolvePlayerGrid(MapCoordinates pos)
    {
        if (Exists(PlayerGrid) && !PlayerGrid.Value.Comp.Deleted &&
            _xform.GetMapId(PlayerGrid.Value.Owner) == pos.MapId)
            return PlayerGrid.Value;

        if (_mapManager.TryFindGridAt(pos, out var grid, out var gridComp))
            PlayerGrid = (grid, gridComp);
        else
            PlayerGrid = null;

        return PlayerGrid;
    }

    private void RemoveBlocker(Entity<SoundBlockerComponent> blocker)
    {
        _shouldResetRaycastFully = true;

        if (blocker.Comp.Indices is { } blockerIndices)
            AddOrRemoveBlocker(blocker, blockerIndices, false, true);
    }

    private void PlayerMoved(EntityUid player, MapCoordinates oldPos, MapCoordinates newPos)
    {
        if (_raycastEnabled)
            _shouldResetRaycastFully = true;

        if (!_pathfindingEnabled)
            return;

        if (newPos == MapCoordinates.Nullspace)
            return;

        if (oldPos.MapId != newPos.MapId || !Exists(PlayerGrid))
        {
            PlayerGrid = null;
            OldPlayerTile = null;
            if (_mapManager.TryFindGridAt(newPos, out var g, out var gC))
            {
                PlayerGrid = (g, gC);
                var tile = _map.TileIndicesFor((g, gC), newPos);
                Expand(tile);
                ReCalculateAllAudio(player,
                    AudioProcessBehavior.None,
                    AudioProcessBehavior.Reset | AudioProcessBehavior.Recalculate);
                return;
            }

            ResetImmediate(player);
            return;
        }

        if (!_mapManager.TryFindGridAt(newPos, out var grid, out var gridComp))
        {
            PlayerGrid = null;
            OldPlayerTile = null;
            return;
        }

        var tileNew = _map.TileIndicesFor((grid, gridComp), newPos);

        if (grid != PlayerGrid.Value.Owner)
        {
            PlayerGrid = (grid, gridComp);
            Expand(tileNew);
            ReCalculateAllAudio(player,
                AudioProcessBehavior.None,
                AudioProcessBehavior.Reset | AudioProcessBehavior.Recalculate);
            return;
        }

        if (oldPos == MapCoordinates.Nullspace)
        {
            Expand(tileNew);
            ReCalculateAllAudio(player,
                AudioProcessBehavior.None,
                AudioProcessBehavior.Reset | AudioProcessBehavior.Recalculate);
            return;
        }

        var tileOld = _map.TileIndicesFor((grid, gridComp), oldPos);

        if (tileOld == tileNew)
        {
            if (OldPlayerTile != null && OldPlayerTile != tileNew)
            {
                RebuildAndExpand(tileNew, OldPlayerTile.Value);
                OldPlayerTile = tileNew;
                ReCalculateAllAudio(player, AudioProcessBehavior.None, AudioProcessBehavior.Reset);
                return;
            }

            ReCalculateAllAudio(player, AudioProcessBehavior.None, AudioProcessBehavior.Reset);
            return;
        }

        OldPlayerTile = tileNew;
        RebuildAndExpand(tileNew, tileOld);
        ReCalculateAllAudio(player, AudioProcessBehavior.None, AudioProcessBehavior.Reset);
    }

    private void ResetBlockerMuffle(EntityUid player,
        Entity<TransformComponent?, SoundBlockerComponent?> blocker,
        MapCoordinates? oldPosition = null,
        MapCoordinates? newPosition = null)
    {
        if (!_pathfindingEnabled)
            return;

        if (!Resolve(blocker, ref blocker.Comp1, ref blocker.Comp2, false))
            return;

        Entity<SoundBlockerComponent> blockerEnt = (blocker.Owner, blocker.Comp2);

        var playerXform = Transform(player);
        var blockerXform = blocker.Comp1;

        var blockerPos = newPosition;
        if (blockerPos == null || blockerPos == MapCoordinates.Nullspace)
            blockerPos = oldPosition;
        if (blockerPos == null || blockerPos == MapCoordinates.Nullspace)
            blockerPos = _xform.GetMapCoordinates(blocker.Owner, blockerXform);
        if (blockerPos == MapCoordinates.Nullspace)
        {
            RemoveBlocker(blockerEnt);
            return;
        }

        var pos = _xform.GetMapCoordinates(player, playerXform);

        var oldIndices = blockerEnt.Comp.Indices;

        if (pos == MapCoordinates.Nullspace)
        {
            if (!Exists(PlayerGrid) || PlayerGrid.Value.Comp.Deleted ||
                _xform.GetMapId(PlayerGrid.Value.Owner) != blockerPos.Value.MapId)
                return;

            ResetBlockerOnGrid(PlayerGrid.Value, blockerEnt, blockerPos.Value, oldIndices);
            return;
        }

        if (pos.MapId != blockerPos.Value.MapId)
        {
            if (blockerEnt.Comp.Indices is { } indices)
                oldIndices = indices;

            if (oldIndices == null)
                return;

            AddOrRemoveBlocker(blockerEnt, oldIndices.Value, false, true);
            return;
        }

        if (TryFindCommonPlayerGrid(pos, blockerPos.Value) is { } grid)
            ResetBlockerOnGrid(grid, blockerEnt, blockerPos.Value, oldIndices);
        else if (oldIndices != null)
            AddOrRemoveBlocker(blockerEnt, oldIndices.Value, false, true);
    }

    private void ResetBlockerOnGrid(Entity<MapGridComponent> grid,
        Entity<SoundBlockerComponent> blocker,
        MapCoordinates blockerPos,
        Vector2i? oldIndices)
    {
        var indices = _map.TileIndicesFor(grid, blockerPos);
        if (oldIndices != null)
        {
            if (indices == oldIndices.Value)
            {
                if (TileDataDict.TryGetValue(indices, out var data))
                {
                    var curCost = data.TotalCost;
                    var baseCost = (data.Previous?.TotalCost ?? -1f) + 1f;
                    var totalCost = GetTotalTileCost(indices);
                    var sum = baseCost + totalCost;
                    var delta = sum - curCost;
                    if (MathHelper.CloseToPercent(delta, 0f))
                        return;

                    ModifyBlockerAmount(data, delta);
                }

                return;
            }

            AddOrRemoveBlocker(blocker, oldIndices.Value, false, true);
        }

        AddOrRemoveBlocker(blocker, indices, true, true);
    }

    private void ReCalculateAudioMuffle(EntityUid player,
        Entity<AudioComponent?, AudioMuffleComponent?> audio,
        MapCoordinates audioPos,
        bool reset = true,
        bool ignoreShort = true)
    {
        if (!Resolve(audio, ref audio.Comp1, ref audio.Comp2, false))
            return;

        Entity<AudioComponent, AudioMuffleComponent> audioEnt = (audio, audio.Comp1, audio.Comp2);

        var playerPos = _xform.GetMapCoordinates(player);

        if (audioPos.MapId != playerPos.MapId)
        {
            RemoveAudioMuffle((audio, audio.Comp2));
            return;
        }

        if (_pathfindingEnabled)
        {
            if (TryFindCommonPlayerGrid(playerPos, audioPos) is { } grid)
            {
                var audioIndices = _map.TileIndicesFor(grid, audioPos);
                if (audio.Comp2.Indices is { } oldIndices)
                {
                    if (audioIndices == oldIndices)
                    {
                        if (reset)
                            ResetAudioMuffle(audio, ignoreShort);
                        return;
                    }

                    if (ReverseAudioPosDict.TryGetValue(oldIndices, out var oldSet))
                    {
                        oldSet.Remove(audioEnt);
                        if (oldSet.Count == 0)
                            ReverseAudioPosDict.Remove(oldIndices);
                    }
                }

                audio.Comp2.Indices = audioIndices;
                if (ReverseAudioPosDict.TryGetValue(audioIndices, out var audioSet))
                    audioSet.Add(audioEnt);
                else
                {
                    ReverseAudioPosDict[audioIndices] = new HashSet<Entity<AudioComponent, AudioMuffleComponent>>
                        { audioEnt };
                }

                if (reset)
                    ResetAudioMuffle(audio, ignoreShort);
                return;
            }
        }

        if (!_raycastEnabled)
            return;

        if (audioPos.Position.EqualsApprox(playerPos.Position))
        {
            if (reset)
                ResetAudioMuffle(audio, ignoreShort);
            return;
        }

        if (audio.Comp2.Indices is { } audioCoords &&
            ReverseAudioPosDict.TryGetValue(audioCoords, out var set))
        {
            set.Remove(audioEnt);
            if (set.Count == 0)
                ReverseAudioPosDict.Remove(audioCoords);
        }

        var dir = audioPos.Position - playerPos.Position;
        var len = dir.Length();
        var norm = dir / len;

        var range = MathF.Min(len, AudioRange);

        var ray = new CollisionRay(playerPos.Position, norm, (int) CollisionGroup.Impassable);
        var result = _physics.IntersectRay(playerPos.MapId, ray, range, player, false);

        var hashSet = audioEnt.Comp2.RayBlockers;
        hashSet.Clear();
        foreach (var hitResult in result)
        {
            var potentialBlocker = hitResult.HitEntity;

            if (potentialBlocker == audio.Owner)
                continue;

            if (!_blockerQuery.TryComp(potentialBlocker, out var blockerComp))
                continue;

            if (_xform.GetWorldPosition(potentialBlocker).EqualsApprox(audioPos.Position))
                continue;

            Entity<SoundBlockerComponent> blockerEnt = (potentialBlocker, blockerComp);

            hashSet.Add(blockerEnt);
        }

        if (reset)
            ResetAudioMuffle(audio, ignoreShort);
    }

    public Entity<MapGridComponent>? TryFindCommonPlayerGrid(MapCoordinates pos, MapCoordinates other)
    {
        if (ResolvePlayerGrid(pos) is { } grid &&
            _mapManager.TryFindGridAt(other, out var gridB, out _) && grid.Owner == gridB)
            return grid;

        return null;
    }

    private void ResetAudioMuffle(Entity<AudioComponent?, AudioMuffleComponent?> audio, bool ignoreShort = false)
    {
        if (!Exists(audio) || !Resolve(audio, ref audio.Comp1, ref audio.Comp2, false))
            return;

        if (!CanMuffle(audio.Comp1))
            return;

        // This should be nuked but without this audio clips when moving or when player is moving
        if (!ignoreShort)
        {
            var offset = ((audio.Comp1.PauseTime ?? _timing.CurTime) - audio.Comp1.AudioStart).TotalSeconds;
            if (offset < ShortAudioLength)
                return;
        }

        if (audio.Comp1.State == AudioState.Stopped || !audio.Comp1.Loaded ||
            ResolvePlayer() is not { } player)
            return;

        var muffleLevel = 0f;
        var xform = Transform(player);
        var playerPos = _xform.GetMapCoordinates(player, xform);

        // ResolvePlayer returns nearest entity that provides ai vision, if it cannot find any, it returns ai eye
        // itself, which means no cameras nearby => all audio is muffled
        if (_aiEyeQuery.HasComp(player) ||
            ManhattanDistance(_xform.GetWorldPosition(audio), playerPos.Position) > AudioRange)
            muffleLevel = 16f;
        else if (_pathfindingEnabled && ResolvePlayerGrid(playerPos) is { } grid &&
                 audio.Comp2.Indices is { } pos && TileDataDict.TryGetValue(pos, out var tileData))
        {
            var playerIndices = _map.TileIndicesFor(grid, playerPos);
            var playerDist = (float) ManhattanDistance(pos, playerIndices);
            muffleLevel = tileData.TotalCost + (playerDist - AudioRange) / 4f - GetTotalTileCost(pos);
            var playerTilePos = _map.GridTileToWorldPos(grid, grid, playerIndices);
            var diff1 = playerPos.Position - playerTilePos;
            var diff2 = (Vector2) (playerIndices - pos);
            var len = diff2.Length();
            if (!MathHelper.CloseToPercent(len, 0f))
                muffleLevel += Vector2.Dot(diff1, diff2) / len;
        }
        else if (_raycastEnabled)
        {
            var data = audio.Comp2.RayBlockers;
            _blockersToRemove.Clear();
            foreach (var blocker in data)
            {
                if (!Exists(blocker))
                {
                    _blockersToRemove.Add(blocker);
                    continue;
                }

                muffleLevel += GetBlockerCost(blocker.Comp);
            }

            foreach (var remove in _blockersToRemove)
            {
                remove.Comp.Indices = null;
                data.Remove(remove);
            }

            _blockersToRemove.Clear();
        }
        else
            muffleLevel = 0f;

        SetVolume(audio, audio.Comp2.OriginalVolume ?? audio.Comp1.Params.Volume, muffleLevel);
    }

    private void ResetAudioOnPos(Vector2i pos)
    {
        if (!ReverseAudioPosDict.TryGetValue(pos, out var audioSet))
            return;

        _audioToRemove.Clear();
        foreach (var audio in audioSet)
        {
            if (!Exists(audio))
            {
                _audioToRemove.Add(audio);
                continue;
            }

            ResetAudioMuffle(audio.AsNullable());
        }

        foreach (var remove in _audioToRemove)
        {
            audioSet.Remove(remove);
        }

        _audioToRemove.Clear();

        if (audioSet.Count == 0)
            ReverseAudioPosDict.Remove(pos);
    }

    public float GetTotalTileCost(Vector2i tile)
    {
        if (!ReverseBlockerIndicesDict.TryGetValue(tile, out var blockers))
            return 0f;

        var total = 0f;
        _blockersToRemove.Clear();
        foreach (var blocker in blockers)
        {
            if (!Exists(blocker))
            {
                _blockersToRemove.Add(blocker);
                continue;
            }

            total += GetBlockerCost(blocker.Comp);
        }

        foreach (var remove in _blockersToRemove)
        {
            remove.Comp.Indices = null;
            blockers.Remove(remove);
        }

        _blockersToRemove.Clear();

        if (blockers.Count == 0)
            ReverseBlockerIndicesDict.Remove(tile);

        return total;
    }

    public static float GetBlockerCost(SoundBlockerComponent blocker)
    {
        if (!blocker.Active)
            return 0f;

        if (blocker.CachedBlockerCost == null)
        {
            var percent = MathF.Max(blocker.SoundBlockPercent, 0f);
            blocker.CachedBlockerCost = percent > 0.99f ? 400f : -(1f / (percent - 1f)) * 4f - 4f;
        }

        return blocker.CachedBlockerCost.Value;
    }

    private void SetVolume(Entity<AudioComponent?> audio, float volume, float muffleLevel)
    {
        if (TerminatingOrDeleted(audio))
            return;

        if (!Resolve(audio, ref audio.Comp, false))
            return;

        switch (muffleLevel)
        {
            case <= 0f:
                break;
            case >= 16f:
                volume = -100f;
                break;
            default:
                var gain = SharedAudioSystem.VolumeToGain(volume) / MathF.Pow(muffleLevel / 16f + 1f, 4f);
                volume = SharedAudioSystem.GainToVolume(gain);
                break;
        }

        _audio.SetVolume(audio, volume, audio);
    }

    private static bool CanMuffle(AudioComponent audio)
    {
        if (audio.LifeStage > ComponentLifeStage.Running)
            return false;

        // For some reason looping doesn't work with audio muffle
        return audio is { Global: false, Looping: false };
    }
}
