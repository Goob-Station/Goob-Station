// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus
//
// SPDX-License-Identifier: MPL-2.0

using System.Diagnostics.Contracts;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Content.Client.Light.EntitySystems;
using Content.Goobstation.Common.CCVar;
using Content.Shared.Light.Components;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Robust.Client.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Goobstation.Client.Audio;

/// <summary>
///     Handles making sounds 'echo' in large, open spaces. Uses simplified raytracing.
/// </summary>
// could use RaycastSystem but the api it has isn't very amazing
public sealed class AreaEchoSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly AudioEffectSystem _audioEffectSystem = default!;
    [Dependency] private readonly RoofSystem _roofSystem = default!;
    [Dependency] private readonly TurfSystem _turfSystem = default!;

    /// <summary>
    ///     The directions that are raycasted to determine size for echo.
    ///         Used relative to the grid.
    /// </summary>
    private Angle[] _calculatedDirections = [Direction.North.ToAngle(), Direction.West.ToAngle(), Direction.South.ToAngle(), Direction.East.ToAngle()];

    /// <summary>
    ///     Values for the minimum arbitrary size at which a certain audio preset
    ///         is picked for sounds. The higher the highest distance here is,
    ///         the generally more calculations it has to do.
    /// </summary>
    /// <remarks>
    ///     Keep in ascending order.
    /// </remarks>
    private static readonly List<(float, ProtoId<AudioPresetPrototype>)> DistancePresets = new() { (12f, "Hallway"), (20f, "Auditorium"), (30f, "ConcertHall"), (40f, "Hangar") };

    /// <summary>
    ///     When is the next time we should check all audio entities and see if they are eligible to be updated.
    /// </summary>
    private TimeSpan _nextExistingUpdate = TimeSpan.Zero;

    /// <summary>
    ///     Collision mask for echoes.
    /// </summary>
    private int _echoLayer = (int) (CollisionGroup.Opaque | CollisionGroup.Impassable); // this could be better but whatever

    private int _echoMaxReflections;
    private bool _echoEnabled = true;
    private TimeSpan _calculationInterval = TimeSpan.FromSeconds(15); // how often we should check existing audio re-apply or remove echo from them when necessary
    private float _calculationalFidelity;

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<RoofComponent> _roofQuery;

    public override void Initialize()
    {
        base.Initialize();

        _configurationManager.OnValueChanged(GoobCVars.AreaEchoReflectionCount, x => _echoMaxReflections = x, invokeImmediately: true);

        _configurationManager.OnValueChanged(GoobCVars.AreaEchoEnabled, x => _echoEnabled = x, invokeImmediately: true);
        _configurationManager.OnValueChanged(GoobCVars.AreaEchoHighResolution, x => _calculatedDirections = GetEffectiveDirections(x), invokeImmediately: true);

        _configurationManager.OnValueChanged(GoobCVars.AreaEchoRecalculationInterval, x => _calculationInterval = x, invokeImmediately: true);
        _configurationManager.OnValueChanged(GoobCVars.AreaEchoStepFidelity, x => _calculationalFidelity = x, invokeImmediately: true);

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _roofQuery = GetEntityQuery<RoofComponent>();

        SubscribeLocalEvent<AudioComponent, EntParentChangedMessage>(OnAudioParentChanged);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_echoEnabled ||
            _gameTiming.CurTime < _nextExistingUpdate)
            return;

        _nextExistingUpdate = _gameTiming.CurTime + _calculationInterval;

        var minimumMagnitude = DistancePresets.TryFirstOrNull(out var first) ? first.Value.Item1 : 0f;
        DebugTools.Assert(minimumMagnitude > 0f, "First distance preset was less than or equal to 0!");
        if (minimumMagnitude <= 0f)
            return;

        var maximumMagnitude = DistancePresets.Last().Item1;
        var audioEnumerator = EntityQueryEnumerator<AudioComponent>();

        while (audioEnumerator.MoveNext(out var uid, out var audioComponent))
        {
            if (!CanAudioEcho(audioComponent) ||
                !audioComponent.Playing)
                continue;

            ProcessAudioEntity((uid, audioComponent), Transform(uid), minimumMagnitude, maximumMagnitude);
        }
    }

    /// <summary>
    ///     Returns all four cardinal directions when <paramref name="highResolution"/> is false.
    ///         Otherwise, returns all eight intercardinal and cardinal directions as listed in
    ///         <see cref="DirectionExtensions.AllDirections"/>. 
    /// </summary>
    [Pure]
    public static Angle[] GetEffectiveDirections(bool highResolution)
    {
        if (highResolution)
        {
            var allDirections = DirectionExtensions.AllDirections;
            var directions = new Angle[allDirections.Length];

            for (var i = 0; i < allDirections.Length; i++)
                directions[i] = allDirections[i].ToAngle();

            return directions;
        }

        return [Direction.North.ToAngle(), Direction.West.ToAngle(), Direction.South.ToAngle(), Direction.East.ToAngle()];
    }

    /// <summary>
    ///     Takes an entity's <see cref="TransformComponent"/>. Goes through every parent it
    ///         has before reaching one that is a map. Returns the hierarchy
    ///         discovered, which includes the given <paramref name="originEntity"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private List<Entity<TransformComponent>> TryGetHierarchyBeforeMap(Entity<TransformComponent> originEntity)
    {
        var hierarchy = new List<Entity<TransformComponent>>() { originEntity };

        ref var currentEntity = ref originEntity;
        ref var currentTransformComponent = ref currentEntity.Comp;

        var mapUid = currentEntity.Comp.MapUid;

        while (currentTransformComponent.ParentUid != mapUid /* break when the next entity is a map... */ &&
            currentTransformComponent.ParentUid.IsValid() /* ...or invalid */ )
        {
            // iterate to next entity
            var nextUid = currentTransformComponent.ParentUid;
            currentEntity.Owner = nextUid;
            currentTransformComponent = Transform(nextUid);

            hierarchy.Add(currentEntity);
        }

        DebugTools.Assert(hierarchy.Count >= 1, "Malformed entity hierarchy! Hierarchy must always contain one element, but it doesn't. How did this happen?");
        return hierarchy;
    }

    /// <summary>
    ///     Basic check for whether an audio can echo. Doesn't account for distance.
    /// </summary>
    public bool CanAudioEcho(AudioComponent audioComponent)
        => !audioComponent.Global && _echoEnabled;

    /// <summary>
    ///     Gets the length of the direction that reaches the furthest unobstructed
    ///         distance, in an attempt to get the size of the area. Aborts early
    ///         if either grid is missing or the tile isnt rooved.
    /// 
    ///     Returned magnitude is the longest valid length of the ray in each direction,
    ///         divided by the number of total processed angles.
    /// </summary>
    /// <returns>Whether anything was actually processed.</returns>
    // i am the total overengineering guy... and this, is my code.
    /*
        This works under a few assumptions:
        - An entity in space is invalid
        - Any spaced tile is invalid
        - Rays end on invalid tiles (space) or unrooved tiles, and dont process on separate grids.
        - - This checked every `_calculationalFidelity`-ish tiles. Not precisely. But somewhere around that. Its moreso just proportional to that.
        - Rays bounce.
    */
    public bool TryProcessAreaSpaceMagnitude(Entity<TransformComponent> entity, float maximumMagnitude, out float magnitude)
    {
        magnitude = 0f;
        var transformComponent = entity.Comp;

        // get either the grid or other parent entity this entity is on, and it's rotation
        var entityHierarchy = TryGetHierarchyBeforeMap(entity);
        if (entityHierarchy.Count <= 1) // hierarchy always starts with our entity. if it only has our entity, it means the next parent was the map, which we don't want
            return false; // means this entity is in space/otherwise not on a grid

        // at this point, we know that we are somewhere on a grid

        // e.g.: if a sound is inside a crate, this will now be the grid the crate is on; if the sound is just on the grid, this will be the grid that the sound is on.
        var entityGrid = entityHierarchy.Last();

        // this is the last entity, or this entity itself, that this entity has, before the parent is a grid/map. e.g.: if a sound is inside a crate, this will be the crate; if the sound is just on the grid, this will be the sound
        var lastEntityBeforeGrid = entityHierarchy[^2]; // `l[^x]` is analogous to `l[l.Count - x]`
        // `lastEntityBeforeGrid` is obviously directly before `entityGrid`
        // the earlier guard clause makes sure this will always be valid

        if (!_gridQuery.TryGetComponent(entityGrid, out var gridComponent))
            return false;

        var checkRoof = _roofQuery.TryGetComponent(entityGrid, out var roofComponent);
        var tileRef = _mapSystem.GetTileRef(entityGrid, gridComponent, lastEntityBeforeGrid.Comp.Coordinates);

        if (tileRef.Tile.IsEmpty)
            return false;

        var gridRoofEntity = new Entity<MapGridComponent, RoofComponent?>(entityGrid, gridComponent, roofComponent);
        if (checkRoof &&
            !_roofSystem.IsRooved(gridRoofEntity!, tileRef.GridIndices))
            return false;

        var originTileIndices = tileRef.GridIndices;
        var worldPosition = _transformSystem.GetWorldPosition(transformComponent);

        // At this point, we are ready for war against the client's pc.
        foreach (var direction in _calculatedDirections)
        {
            var currentDirectionVector = direction.ToVec();
            var currentTargetEntityUid = lastEntityBeforeGrid.Owner;

            var totalDistance = 0f;
            var remainingDistance = maximumMagnitude;

            var currentOriginWorldPosition = worldPosition;
            var currentOriginTileIndices = originTileIndices;

            for (var reflectIteration = 0; reflectIteration <= _echoMaxReflections /* if maxreflections is 0 we still cast atleast once */; reflectIteration++)
            {
                var (distanceCovered, raycastResults) = CastEchoRay(
                    currentOriginWorldPosition,
                    currentOriginTileIndices,
                    currentDirectionVector,
                    transformComponent.MapID,
                    currentTargetEntityUid,
                    gridRoofEntity,
                    checkRoof,
                    remainingDistance
                );

                totalDistance += distanceCovered;
                remainingDistance -= distanceCovered;

                // we don't need further logic anyway if we just finished the last iteration
                if (reflectIteration == _echoMaxReflections)
                    break;

                if (raycastResults is not { }) // means we didnt hit anything
                    break;

                // i think cross-grid would actually be pretty easy here? but the tile-marching doesnt often account for that at fidelities above 1 so whatever.

                var previousRayWorldOriginPosition = currentOriginWorldPosition;
                currentOriginWorldPosition = raycastResults.Value.HitPos; // it's now where we hit
                currentTargetEntityUid = raycastResults.Value.HitEntity;

                if (!_mapSystem.TryGetTileRef(entityGrid, gridComponent, currentOriginWorldPosition, out var hitTileRef)) // means tile that ray hit is invalid, just assume the ray ends here
                    break;

                currentOriginTileIndices = hitTileRef.GridIndices;

                var worldMatrix = _transformSystem.GetInvWorldMatrix(gridRoofEntity);
                var previousRayOriginLocalPosition = Vector2.Transform(previousRayWorldOriginPosition, worldMatrix);
                var currentOriginLocalPosition = Vector2.Transform(currentOriginWorldPosition, worldMatrix);

                var delta = currentOriginLocalPosition - previousRayOriginLocalPosition;
                if (delta.LengthSquared() <= float.Epsilon + float.Epsilon)
                {
                    Log.Warning($"Echo-ray has the same origin position as its hit position! At: {_mapSystem.WorldToLocal(gridRoofEntity, gridRoofEntity, currentOriginLocalPosition)}, on entity: {ToPrettyString(gridRoofEntity.Owner)}");
                    break;
                }

                var normalVector = GetNormalVector(delta);
                normalVector = GetTileHitNormal(currentOriginLocalPosition, _mapSystem.TileToVector(gridRoofEntity, currentOriginTileIndices), gridRoofEntity.Comp1.TileSize);
                currentDirectionVector = Reflect(currentDirectionVector, normalVector);
            }

            magnitude += totalDistance;
        }

        magnitude /= _calculatedDirections.Length * _echoMaxReflections;
        return true;
    }

    /// <summary>
    ///     Gets the normal angle of a Vector2, relative to
    ///         0, 0.
    /// </summary>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 GetNormalVector(in Vector2 deltaVector)
    {
        return Vector2.Normalize(
            MathF.Abs(deltaVector.X) > MathF.Abs(deltaVector.Y) ?
            new Vector2(MathF.Sign(deltaVector.X), 0f) :
            new Vector2(0f, MathF.Sign(deltaVector.Y))
        );
    }

    Vector2 GetTileHitNormal(Vector2 rayHitPos, Vector2 tileOrigin, float tileSize)
    {
        // Position inside the tile (0..tileSize)
        Vector2 local = rayHitPos - tileOrigin;

        // Distances to each side
        float left = local.X;
        float right = tileSize - local.X;
        float bottom = local.Y;
        float top = tileSize - local.Y;

        // Find smallest distance
        float minDist = MathF.Min(MathF.Min(left, right), MathF.Min(bottom, top));

        if (minDist == left) return new Vector2(-1, 0);
        if (minDist == right) return new Vector2(1, 0);
        if (minDist == bottom) return new Vector2(0, -1);
        return new Vector2(0, 1); // must be top
    }

    /// <remarks>
    ///     <paramref name="normal"/> should be normalised upon calling.
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2 Reflect(in Vector2 direction, in Vector2 normal)
        => direction - 2 * Vector2.Dot(direction, normal) * normal;

    // this caused vsc to spike to 28gb memory usage
    /// <summary>
    ///     Casts a ray and marches it. See <see cref="MarchRayByTiles"/>. 
    /// </summary>
    private (float, RayCastResults?) CastEchoRay(
        in Vector2 originWorldPosition,
        in Vector2i originTileIndices,
        in Vector2 directionVector,
        in MapId mapId,
        in EntityUid ignoredEntity,
        in Entity<MapGridComponent, RoofComponent?> gridRoofEntity,
        bool checkRoof,
        float maximumDistance
    )
    {
        var directionFidelityStep = directionVector * _calculationalFidelity;

        var ray = new CollisionRay(originWorldPosition, directionVector, _echoLayer);
        var rayResults = _physicsSystem.IntersectRay(mapId, ray, maxLength: maximumDistance, ignoredEnt: ignoredEntity, returnOnFirstHit: true);

        // if we hit something, distance to that is magnitude but it must be lower than maximum. if we didnt hit anything, it's maximum magnitude
        var rayMagnitude = rayResults.TryFirstOrNull(out var firstResult) ?
            MathF.Min(firstResult.Value.Distance, maximumDistance) :
            maximumDistance;

        var nextCheckedPosition = new Vector2(originTileIndices.X, originTileIndices.Y) * gridRoofEntity.Comp1.TileSize + directionFidelityStep;
        var incrementedRayMagnitude = MarchRayByTiles(
            rayMagnitude,
            gridRoofEntity,
            directionFidelityStep,
            ref nextCheckedPosition,
            gridRoofEntity.Comp1.TileSize,
            checkRoof
        );

        return (incrementedRayMagnitude, firstResult);
    }

    /// <summary>
    ///     Advances a ray, in intervals of `_calculationalFidelity`, by tiles until
    ///         reaching an unrooved tile (if checking roofs) or space.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float MarchRayByTiles(
        in float rayMagnitude,
        in Entity<MapGridComponent, RoofComponent?> gridRoofEntity,
        in Vector2 directionFidelityStep,
        ref Vector2 nextCheckedPosition,
        ushort gridTileSize,
        bool checkRoof
    )
    {
        // find the furthest distance this ray reaches until its on an unrooved/dataless (space) tile

        var fidelityStepLength = directionFidelityStep.Length();
        var incrementedRayMagnitude = 0f;

        for (; incrementedRayMagnitude < rayMagnitude;)
        {
            var nextCheckedTilePosition = new Vector2i(
                (int) MathF.Floor(nextCheckedPosition.X / gridTileSize),
                (int) MathF.Floor(nextCheckedPosition.Y / gridTileSize)
            );

            if (checkRoof)
            { // if we're checking roofs, end this ray if this tile is unrooved or dataless (latter is inherent of this method)
                if (!_roofSystem.IsRooved(gridRoofEntity!, nextCheckedTilePosition))
                    break;
            } // if we're not checking roofs, end this ray if this tile is empty/space
            else if (!_mapSystem.TryGetTileRef(gridRoofEntity, gridRoofEntity, nextCheckedTilePosition, out var tile) ||
                _turfSystem.IsSpace(tile.Tile))
                break;

            nextCheckedPosition += directionFidelityStep;
            incrementedRayMagnitude += fidelityStepLength;
        }

        return MathF.Min(incrementedRayMagnitude, rayMagnitude);
    }

    private void ProcessAudioEntity(Entity<AudioComponent> entity, TransformComponent transformComponent, float minimumMagnitude, float maximumMagnitude)
    {
        TryProcessAreaSpaceMagnitude((entity, transformComponent), maximumMagnitude, out var echoMagnitude);

        if (echoMagnitude > minimumMagnitude)
        {
            ProtoId<AudioPresetPrototype>? bestPreset = null;
            for (var i = DistancePresets.Count - 1; i >= 0; i--)
            {
                var preset = DistancePresets[i];
                if (preset.Item1 < echoMagnitude)
                    continue;

                bestPreset = preset.Item2;
            }

            if (bestPreset != null)
                _audioEffectSystem.TryAddEffect(entity, DistancePresets[0].Item2);
        }
        else
            _audioEffectSystem.TryRemoveEffect(entity);
    }

    // Maybe TODO: defer this onto ticks? but whatever its just clientside
    private void OnAudioParentChanged(Entity<AudioComponent> entity, ref EntParentChangedMessage args)
    {
        if (args.Transform.MapID == MapId.Nullspace)
            return;

        if (!CanAudioEcho(entity))
            return;

        var minimumMagnitude = DistancePresets.TryFirstOrNull(out var first) ? first.Value.Item1 : 0f;
        DebugTools.Assert(minimumMagnitude > 0f, "First distance preset was less than or equal to 0!");
        if (minimumMagnitude <= 0f)
            return;

        var maximumMagnitude = DistancePresets.Last().Item1;

        ProcessAudioEntity(entity, args.Transform, minimumMagnitude, maximumMagnitude);
    }
}
