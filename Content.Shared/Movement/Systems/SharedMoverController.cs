using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Bed.Sleep;
using Content.Shared.CCVar;
using Content.Shared.Friction;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Controllers;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
// Tile Movement Change
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared._vg.TileMovement;
using PullableComponent = Content.Shared.Movement.Pulling.Components.PullableComponent;

namespace Content.Shared.Movement.Systems;

/// <summary>
///     Handles player and NPC mob movement.
///     NPCs are handled server-side only.
/// </summary>
public abstract partial class SharedMoverController : VirtualController
{
    [Dependency] private   readonly IConfigurationManager _configManager = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private   readonly IMapManager _mapManager = default!;
    [Dependency] private   readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private   readonly EntityLookupSystem _lookup = default!;
    [Dependency] private   readonly InventorySystem _inventory = default!;
    [Dependency] private   readonly MobStateSystem _mobState = default!;
    [Dependency] private   readonly SharedAudioSystem _audio = default!;
    [Dependency] private   readonly SharedContainerSystem _container = default!;
    [Dependency] private   readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private   readonly SharedGravitySystem _gravity = default!;
    [Dependency] protected readonly SharedPhysicsSystem Physics = default!;
    [Dependency] private   readonly SharedTransformSystem _transform = default!;
    [Dependency] private   readonly TagSystem _tags = default!;
    [Dependency] private   readonly SharedInteractionSystem _interaction = default!; // Tile Movement Change

    protected EntityQuery<InputMoverComponent> MoverQuery;
    protected EntityQuery<MobMoverComponent> MobMoverQuery;
    protected EntityQuery<MovementRelayTargetComponent> RelayTargetQuery;
    protected EntityQuery<MovementSpeedModifierComponent> ModifierQuery;
    protected EntityQuery<PhysicsComponent> PhysicsQuery;
    protected EntityQuery<RelayInputMoverComponent> RelayQuery;
    protected EntityQuery<PullableComponent> PullableQuery;
    protected EntityQuery<TransformComponent> XformQuery;
    protected EntityQuery<CanMoveInAirComponent> CanMoveInAirQuery;
    protected EntityQuery<NoRotateOnMoveComponent> NoRotateQuery;
    protected EntityQuery<FootstepModifierComponent> FootstepModifierQuery;
    protected EntityQuery<MapGridComponent> MapGridQuery;
    protected EntityQuery<FixturesComponent> FixturesQuery; // Tile Movement Change
    protected EntityQuery<TileMovementComponent> TileMovementQuery; // Tile Movement Change
    /// <summary>
    /// <see cref="CCVars.StopSpeed"/>
    /// </summary>
    private float _stopSpeed;

    private bool _relativeMovement;
    private TimeSpan CurrentTime => PhysicsSystem.EffectiveCurTime ?? Timing.CurTime; // Tile Movement Change

    /// <summary>
    /// Cache the mob movement calculation to re-use elsewhere.
    /// </summary>
    public Dictionary<EntityUid, bool> UsedMobMovement = new();

    public override void Initialize()
    {
        base.Initialize();

        MoverQuery = GetEntityQuery<InputMoverComponent>();
        MobMoverQuery = GetEntityQuery<MobMoverComponent>();
        ModifierQuery = GetEntityQuery<MovementSpeedModifierComponent>();
        RelayTargetQuery = GetEntityQuery<MovementRelayTargetComponent>();
        PhysicsQuery = GetEntityQuery<PhysicsComponent>();
        RelayQuery = GetEntityQuery<RelayInputMoverComponent>();
        PullableQuery = GetEntityQuery<PullableComponent>();
        XformQuery = GetEntityQuery<TransformComponent>();
        NoRotateQuery = GetEntityQuery<NoRotateOnMoveComponent>();
        CanMoveInAirQuery = GetEntityQuery<CanMoveInAirComponent>();
        FootstepModifierQuery = GetEntityQuery<FootstepModifierComponent>();
        MapGridQuery = GetEntityQuery<MapGridComponent>();
        FixturesQuery = GetEntityQuery<FixturesComponent>(); // Tile Movement Change
        TileMovementQuery = GetEntityQuery<TileMovementComponent>(); // Tile Movement Change
        InitializeInput();
        InitializeRelay();
        Subs.CVar(_configManager, CCVars.RelativeMovement, value => _relativeMovement = value, true);
        Subs.CVar(_configManager, CCVars.StopSpeed, value => _stopSpeed = value, true);
        UpdatesBefore.Add(typeof(TileFrictionController));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        ShutdownInput();
    }

    public override void UpdateAfterSolve(bool prediction, float frameTime)
    {
        base.UpdateAfterSolve(prediction, frameTime);
        UsedMobMovement.Clear();
    }

    /// <summary>
    ///     Movement while considering actionblockers, weightlessness, etc.
    /// </summary>
    protected void HandleMobMovement(
        EntityUid uid,
        InputMoverComponent mover,
        EntityUid physicsUid,
        PhysicsComponent physicsComponent,
        TransformComponent xform,
        float frameTime)
    {
        var canMove = mover.CanMove;
        if (RelayTargetQuery.TryGetComponent(uid, out var relayTarget))
        {
            if (_mobState.IsIncapacitated(relayTarget.Source) ||
                TryComp<SleepingComponent>(relayTarget.Source, out _) ||
                // Shitmed Change
                !PhysicsQuery.TryGetComponent(relayTarget.Source, out var relayedPhysicsComponent) ||
                !MoverQuery.TryGetComponent(relayTarget.Source, out var relayedMover) ||
                !XformQuery.TryGetComponent(relayTarget.Source, out var relayedXform))
            {
                canMove = false;
            }
            else
            {
                mover.LerpTarget = relayedMover.LerpTarget;
                mover.RelativeEntity = relayedMover.RelativeEntity;
                mover.RelativeRotation = relayedMover.RelativeRotation;
                mover.TargetRelativeRotation = relayedMover.TargetRelativeRotation;
                HandleMobMovement(relayTarget.Source, relayedMover, relayTarget.Source, relayedPhysicsComponent, relayedXform, frameTime);
            }
        }

        // Update relative movement
        // Shitmed Change Start
        else
        {
            if (mover.LerpTarget < Timing.CurTime)
            {
                if (TryComp(uid, out RelayInputMoverComponent? relay)
                    && TryComp(relay.RelayEntity, out TransformComponent? relayXform))
                {
                    if (TryUpdateRelative(mover, relayXform))
                        Dirty(uid, mover);
                }
                else
                {
                    if (TryUpdateRelative(mover, xform))
                        Dirty(uid, mover);
                }
            }
            LerpRotation(uid, mover, frameTime);
        }
        // Shitmed Change End

        if (!canMove
            || physicsComponent.BodyStatus != BodyStatus.OnGround && !CanMoveInAirQuery.HasComponent(uid)
            || PullableQuery.TryGetComponent(uid, out var pullable) && pullable.BeingPulled)
        {
            UsedMobMovement[uid] = false;
            return;
        }


        UsedMobMovement[uid] = true;
        // Specifically don't use mover.Owner because that may be different to the actual physics body being moved.
        var weightless = _gravity.IsWeightless(physicsUid, physicsComponent, xform);
        var (walkDir, sprintDir) = GetVelocityInput(mover);
        var touching = false;

        // Handle wall-pushes.
        if (weightless)
        {
            if (xform.GridUid != null)
                touching = true;

            if (!touching)
            {
                var ev = new CanWeightlessMoveEvent(uid);
                RaiseLocalEvent(uid, ref ev, true);
                // No gravity: is our entity touching anything?
                touching = ev.CanMove;

                if (!touching && TryComp<MobMoverComponent>(uid, out var mobMover))
                    touching |= IsAroundCollider(PhysicsSystem, xform, mobMover, physicsUid, physicsComponent);
            }
        }

        // Get current tile def for things like speed/friction mods
        ContentTileDefinition? tileDef = null;

        // Don't bother getting the tiledef here if we're weightless or in-air
        // since no tile-based modifiers should be applying in that situation
        if (MapGridQuery.TryComp(xform.GridUid, out var gridComp)
            && _mapSystem.TryGetTileRef(xform.GridUid.Value, gridComp, xform.Coordinates, out var tile)
            && !(weightless || physicsComponent.BodyStatus == BodyStatus.InAir))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tile.Tile.TypeId];
        }

        // Tile Movement Change
        // Try doing tile movement.
        if (TileMovementQuery.TryComp(physicsUid, out var tileMovement))
        {
            if (!weightless && physicsComponent.BodyStatus == BodyStatus.OnGround)
            {
                var didTileMovement = HandleTileMovement(uid,
                    physicsUid,
                    tileMovement,
                    physicsComponent,
                    xform,
                    mover,
                    tileDef,
                    relayTarget,
                    frameTime);
                tileMovement.WasWeightlessLastTick = weightless;
                if(didTileMovement)
                {
                    return;
                }
            }
            else
            {
                tileMovement.WasWeightlessLastTick = weightless;
                tileMovement.SlideActive = false;
            }
        }

        // Regular movement.
        // Target velocity.
        // This is relative to the map / grid we're on.
        var moveSpeedComponent = ModifierQuery.CompOrNull(uid);

        var walkSpeed = moveSpeedComponent?.CurrentWalkSpeed ?? MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
        var sprintSpeed = moveSpeedComponent?.CurrentSprintSpeed ?? MovementSpeedModifierComponent.DefaultBaseSprintSpeed;

        var total = walkDir * walkSpeed + sprintDir * sprintSpeed;

        var parentRotation = GetParentGridAngle(mover);
        var worldTotal = _relativeMovement ? parentRotation.RotateVec(total) : total;

        DebugTools.Assert(MathHelper.CloseToPercent(total.Length(), worldTotal.Length()));

        var velocity = physicsComponent.LinearVelocity;
        float friction;
        float weightlessModifier;
        float accel;

        if (weightless)
        {
            if (gridComp == null && !MapGridQuery.HasComp(xform.GridUid))
                friction = moveSpeedComponent?.OffGridFriction ?? MovementSpeedModifierComponent.DefaultOffGridFriction;
            else if (worldTotal != Vector2.Zero && touching)
                friction = moveSpeedComponent?.WeightlessFriction ?? MovementSpeedModifierComponent.DefaultWeightlessFriction;
            else
                friction = moveSpeedComponent?.WeightlessFrictionNoInput ?? MovementSpeedModifierComponent.DefaultWeightlessFrictionNoInput;

            weightlessModifier = moveSpeedComponent?.WeightlessModifier ?? MovementSpeedModifierComponent.DefaultWeightlessModifier;
            accel = moveSpeedComponent?.WeightlessAcceleration ?? MovementSpeedModifierComponent.DefaultWeightlessAcceleration;
        }
        else
        {
            if (worldTotal != Vector2.Zero || moveSpeedComponent?.FrictionNoInput == null)
            {
                friction = tileDef?.MobFriction ?? moveSpeedComponent?.Friction ?? MovementSpeedModifierComponent.DefaultFriction;
            }
            else
            {
                friction = tileDef?.MobFrictionNoInput ?? moveSpeedComponent.FrictionNoInput ?? MovementSpeedModifierComponent.DefaultFrictionNoInput;
            }

            weightlessModifier = 1f;
            accel = tileDef?.MobAcceleration ?? moveSpeedComponent?.Acceleration ?? MovementSpeedModifierComponent.DefaultAcceleration;
        }

        var minimumFrictionSpeed = moveSpeedComponent?.MinimumFrictionSpeed ?? MovementSpeedModifierComponent.DefaultMinimumFrictionSpeed;
        Friction(minimumFrictionSpeed, frameTime, friction, ref velocity);

        if (worldTotal != Vector2.Zero)
        {
            if (!NoRotateQuery.HasComponent(uid))
            {
                // TODO apparently this results in a duplicate move event because "This should have its event run during
                // island solver"??. So maybe SetRotation needs an argument to avoid raising an event?
                var worldRot = _transform.GetWorldRotation(xform);
                _transform.SetLocalRotation(xform, xform.LocalRotation + worldTotal.ToWorldAngle() - worldRot);
            }

            if (!weightless && MobMoverQuery.TryGetComponent(uid, out var mobMover) &&
                TryGetSound(weightless, uid, mover, mobMover, xform, out var sound, tileDef: tileDef))
            {
                var soundModifier = mover.Sprinting ? 3.5f : 1.5f;

                var audioParams = sound.Params
                    .WithVolume(sound.Params.Volume + soundModifier)
                    .WithVariation(sound.Params.Variation ?? mobMover.FootstepVariation);

                // If we're a relay target then predict the sound for all relays.
                if (relayTarget != null)
                {
                    _audio.PlayPredicted(sound, uid, relayTarget.Source, audioParams);
                }
                else
                {
                    _audio.PlayPredicted(sound, uid, uid, audioParams);
                }
            }
        }

        worldTotal *= weightlessModifier;

        if (!weightless || touching)
            Accelerate(ref velocity, in worldTotal, accel, frameTime);

        PhysicsSystem.SetLinearVelocity(physicsUid, velocity, body: physicsComponent);

        // Ensures that players do not spiiiiiiin
        PhysicsSystem.SetAngularVelocity(physicsUid, 0, body: physicsComponent);
    }

    public void LerpRotation(EntityUid uid, InputMoverComponent mover, float frameTime)
    {
        var angleDiff = Angle.ShortestDistance(mover.RelativeRotation, mover.TargetRelativeRotation);

        // if we've just traversed then lerp to our target rotation.
        if (!angleDiff.EqualsApprox(Angle.Zero, 0.001))
        {
            var adjustment = angleDiff * 5f * frameTime;
            var minAdjustment = 0.01 * frameTime;

            if (angleDiff < 0)
            {
                adjustment = Math.Min(adjustment, -minAdjustment);
                adjustment = Math.Clamp(adjustment, angleDiff, -angleDiff);
            }
            else
            {
                adjustment = Math.Max(adjustment, minAdjustment);
                adjustment = Math.Clamp(adjustment, -angleDiff, angleDiff);
            }

            mover.RelativeRotation += adjustment;
            mover.RelativeRotation.FlipPositive();
            Dirty(uid, mover);
        }
        else if (!angleDiff.Equals(Angle.Zero))
        {
            mover.TargetRelativeRotation.FlipPositive();
            mover.RelativeRotation = mover.TargetRelativeRotation;
            Dirty(uid, mover);
        }
    }

    private void Friction(float minimumFrictionSpeed, float frameTime, float friction, ref Vector2 velocity)
    {
        var speed = velocity.Length();

        if (speed < minimumFrictionSpeed)
            return;

        var drop = 0f;

        var control = MathF.Max(_stopSpeed, speed);
        drop += control * friction * frameTime;

        var newSpeed = MathF.Max(0f, speed - drop);

        if (newSpeed.Equals(speed))
            return;

        newSpeed /= speed;
        velocity *= newSpeed;
    }

    private void Accelerate(ref Vector2 currentVelocity, in Vector2 velocity, float accel, float frameTime)
    {
        var wishDir = velocity != Vector2.Zero ? velocity.Normalized() : Vector2.Zero;
        var wishSpeed = velocity.Length();

        var currentSpeed = Vector2.Dot(currentVelocity, wishDir);
        var addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0f)
            return;

        var accelSpeed = accel * frameTime * wishSpeed;
        accelSpeed = MathF.Min(accelSpeed, addSpeed);

        currentVelocity += wishDir * accelSpeed;
    }

    public bool UseMobMovement(EntityUid uid)
    {
        return UsedMobMovement.TryGetValue(uid, out var used) && used;
    }

    /// <summary>
    ///     Used for weightlessness to determine if we are near a wall.
    /// </summary>
    private bool IsAroundCollider(SharedPhysicsSystem broadPhaseSystem, TransformComponent transform, MobMoverComponent mover, EntityUid physicsUid, PhysicsComponent collider)
    {
        var enlargedAABB = _lookup.GetWorldAABB(physicsUid, transform).Enlarged(mover.GrabRangeVV);

        foreach (var otherCollider in broadPhaseSystem.GetCollidingEntities(transform.MapID, enlargedAABB))
        {
            if (otherCollider == collider)
                continue; // Don't try to push off of yourself!

            // Only allow pushing off of anchored things that have collision.
            if (otherCollider.BodyType != BodyType.Static ||
                !otherCollider.CanCollide ||
                ((collider.CollisionMask & otherCollider.CollisionLayer) == 0 &&
                (otherCollider.CollisionMask & collider.CollisionLayer) == 0) ||
                (TryComp(otherCollider.Owner, out PullableComponent? pullable) && pullable.BeingPulled))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    protected abstract bool CanSound();

    private bool TryGetSound(
        bool weightless,
        EntityUid uid,
        InputMoverComponent mover,
        MobMoverComponent mobMover,
        TransformComponent xform,
        [NotNullWhen(true)] out SoundSpecifier? sound,
        ContentTileDefinition? tileDef = null)
    {
        sound = null;

        if (!CanSound() || !_tags.HasTag(uid, "FootstepSound"))
            return false;

        var coordinates = xform.Coordinates;
        var distanceNeeded = mover.Sprinting
            ? mobMover.StepSoundMoveDistanceRunning
            : mobMover.StepSoundMoveDistanceWalking;

        // Handle footsteps.
        if (!weightless)
        {
            // Can happen when teleporting between grids.
            if (!coordinates.TryDistance(EntityManager, mobMover.LastPosition, out var distance) ||
                distance > distanceNeeded)
            {
                mobMover.StepSoundDistance = distanceNeeded;
            }
            else
            {
                mobMover.StepSoundDistance += distance;
            }
        }
        else
        {
            // In space no one can hear you squeak
            return false;
        }

        mobMover.LastPosition = coordinates;

        if (mobMover.StepSoundDistance < distanceNeeded)
            return false;

        mobMover.StepSoundDistance -= distanceNeeded;

        if (FootstepModifierQuery.TryComp(uid, out var moverModifier))
        {
            sound = moverModifier.FootstepSoundCollection;
            return sound != null;
        }

        if (_inventory.TryGetSlotEntity(uid, "shoes", out var shoes) &&
            FootstepModifierQuery.TryComp(shoes, out var modifier))
        {
            sound = modifier.FootstepSoundCollection;
            return sound != null;
        }

        return TryGetFootstepSound(uid, xform, shoes != null, out sound, tileDef: tileDef);
    }

    private bool TryGetFootstepSound(
        EntityUid uid,
        TransformComponent xform,
        bool haveShoes,
        [NotNullWhen(true)] out SoundSpecifier? sound,
        ContentTileDefinition? tileDef = null)
    {
        sound = null;

        // Fallback to the map?
        if (!MapGridQuery.TryComp(xform.GridUid, out var grid))
        {
            if (FootstepModifierQuery.TryComp(xform.MapUid, out var modifier))
            {
                sound = modifier.FootstepSoundCollection;
            }

            return sound != null;
        }

        var position = grid.LocalToTile(xform.Coordinates);
        var soundEv = new GetFootstepSoundEvent(uid);

        // If the coordinates have a FootstepModifier component
        // i.e. component that emit sound on footsteps emit that sound
        var anchored = grid.GetAnchoredEntitiesEnumerator(position);

        while (anchored.MoveNext(out var maybeFootstep))
        {
            RaiseLocalEvent(maybeFootstep.Value, ref soundEv);

            if (soundEv.Sound != null)
            {
                sound = soundEv.Sound;
                return true;
            }

            if (FootstepModifierQuery.TryComp(maybeFootstep, out var footstep))
            {
                sound = footstep.FootstepSoundCollection;
                return sound != null;
            }
        }

        // Walking on a tile.
        // Tile def might have been passed in already from previous methods, so use that
        // if we have it
        if (tileDef == null && grid.TryGetTileRef(position, out var tileRef))
        {
            tileDef = (ContentTileDefinition) _tileDefinitionManager[tileRef.Tile.TypeId];
        }

        if (tileDef == null)
            return false;

        sound = haveShoes ? tileDef.FootstepSounds : tileDef.BarestepSounds;
        return sound != null;
    }

    // Tile Movement Functions

    /// /vg/station Tile Movement!
    /// Uses a physics-based implementation, resulting in fluid tile movement that mixes the responsiveness of
    /// pixel movement and the rigidity of tiles. Works surprisingly well.
    /// Note: the code is intentionally separated here from everything else to make it easier to port and
    /// to reduce the risk of merge conflicts.
    /// However, I would also NOT recommend porting it right now unless you're okay with continually updating it.
    /// For one, a shapecast-based implementation rather than a true physics implementation is in the cards for
    /// the future. For another, it's not terribly clean and is not integrated too well into existing movement code.

    /// <summary>
    /// Runs one tick of tile-based movement on the given inputs.
    /// </summary>
    /// <param name="uid">UID of the entity doing the move.</param>
    /// <param name="physicsUid">UID of the physics entity doing the move. Usually the same as uid.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity doing the move.</param>
    /// <param name="physicsComponent">PhysicsComponent on the entity doing the move.</param>
    /// <param name="targetTransform">TransformComponent on the entity doing the move.</param>
    /// <param name="inputMover">InputMoverComponent on the entity doing the move.</param>
    /// <param name="tileDef">ContentTileDefinition of the tile underneath the entity doing the move, if there is one.</param>
    /// <param name="relayTarget">MovementRelayTargetComponent on the relay target, if any.</param>
    /// <param name="frameTime">Time in seconds since the last tick of the physics system.</param>
    /// <returns></returns>
    public bool HandleTileMovement(
        EntityUid uid,
        EntityUid physicsUid,
        TileMovementComponent tileMovement,
        PhysicsComponent physicsComponent,
        TransformComponent targetTransform,
        InputMoverComponent inputMover,
        ContentTileDefinition? tileDef,
        MovementRelayTargetComponent? relayTarget,
        float frameTime
    )
    {
        // For smoothness' sake, if we just arrived on a grid after pixel moving in space then initiate a slide
        // towards the center of the tile we're on and continue. It feels much nicer this way.
        if (tileMovement.WasWeightlessLastTick)
        {
            InitializeSlideToCenter(physicsUid, tileMovement);
            UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
        }
        // If we're not moving, apply friction to existing velocity and then continue.
        else if (StripWalk(inputMover.HeldMoveButtons) == MoveButtons.None && !tileMovement.SlideActive)
        {
            var movementVelocity = physicsComponent.LinearVelocity;

            var movementSpeedComponent = ModifierQuery.CompOrNull(uid);
            var friction = GetEntityFriction(inputMover, movementSpeedComponent, tileDef);
            var minimumFrictionSpeed = movementSpeedComponent?.MinimumFrictionSpeed ??
                MovementSpeedModifierComponent.DefaultMinimumFrictionSpeed;
            Friction(minimumFrictionSpeed, frameTime, friction, ref movementVelocity);

            PhysicsSystem.SetLinearVelocity(physicsUid, movementVelocity, body: physicsComponent);
            PhysicsSystem.SetAngularVelocity(physicsUid, 0, body: physicsComponent);
        }
        // Otherwise, handle typical tile movement.
        else
        {
            // Play step sound.
            if (MobMoverQuery.TryGetComponent(uid, out var mobMover) &&
                TryGetSound(false, uid, inputMover, mobMover, targetTransform, out var sound, tileDef: tileDef))
            {
                var soundModifier = inputMover.Sprinting ? 3.5f : 1.5f;
                var volume = sound.Params.Volume + soundModifier;

                var audioParams = sound.Params
                    .WithVolume(volume)
                    .WithVariation(sound.Params.Variation ?? mobMover.FootstepVariation);

                // If we're a relay target then predict the sound for all relays.
                if (relayTarget != null)
                {
                    _audio.PlayPredicted(sound, uid, relayTarget.Source, audioParams);
                }
                else
                {
                    _audio.PlayPredicted(sound, uid, uid, audioParams);
                }
            }

            // If we're sliding...
            if (tileMovement.SlideActive)
            {
                var movementSpeed = GetEntityMoveSpeed(uid, inputMover.Sprinting);

                // Check whether we should end the slide.
                if (CheckForSlideEnd(
                    StripWalk(inputMover.HeldMoveButtons),
                    targetTransform,
                    tileMovement,
                    movementSpeed))
                {
                    EndSlide(uid, tileMovement);

                    // After ending the slide, check for immediately starting a new slide.
                    if (StripWalk(inputMover.HeldMoveButtons) != MoveButtons.None)
                    {
                        InitializeSlide(physicsUid, tileMovement, inputMover);
                        UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
                        tileMovement.FailureSlideActive = false;
                    }
                    // Otherwise if we failed to reach the destination, begin a "failure slide" back to the
                    // original position.
                    else if(!tileMovement.FailureSlideActive && !targetTransform.LocalPosition.EqualsApprox(tileMovement.Destination, 0.04))
                    {
                        InitializeSlideToTarget(physicsUid, tileMovement, targetTransform.LocalPosition, MoveButtons.None);
                        UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
                        tileMovement.FailureSlideActive = true;
                    }
                    // If we reached proper destination or have already done a "failure slide", snap to tile forcefully.
                    else
                    {
                        ForceSnapToTile(uid, inputMover);
                        tileMovement.FailureSlideActive = false;
                    }
                }
                // Special case: tile movement takes us between two fully adjacent grids seamlessly.
                // Since we perform tile movement in local coordinates, stop and start the movement
                // again to realign to new grid.
                // Improvement suggestion: this is mostly smooth but there is a very tiny bit of
                // jitter. Instead of being lazy and stopping/starting a new movement, it should
                // convert the origin into the coordinate system with the new grid as the parent.
                else if (tileMovement.Origin.EntityId != targetTransform.ParentUid)
                {
                    var previousButtons = tileMovement.CurrentSlideMoveButtons;
                    var previousInitialKeyDownTime = tileMovement.MovementKeyInitialDownTime;
                    InitializeSlideToCenter(physicsUid, tileMovement);
                    tileMovement.CurrentSlideMoveButtons = previousButtons;
                    tileMovement.MovementKeyInitialDownTime = previousInitialKeyDownTime;
                    UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
                }
                // Otherwise, continue slide.
                else
                {
                    UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
                }
            }
            // If we're not sliding, start slide.
            else
            {
                InitializeSlide(physicsUid, tileMovement, inputMover);
                UpdateSlide(physicsUid, physicsUid, tileMovement, inputMover);
            }

            // Set WorldRotation so that our character is facing the way we're walking.
            if (!NoRotateQuery.HasComponent(uid) && !tileMovement.FailureSlideActive)
            {
                if (tileMovement.SlideActive && TryComp(
                    inputMover.RelativeEntity,
                    out TransformComponent? parentTransform))
                {
                    var delta = tileMovement.Destination - tileMovement.Origin.Position;
                    var worldRot = _transform.GetWorldRotation(parentTransform).RotateVec(delta).ToWorldAngle();
                    _transform.SetWorldRotation(targetTransform, worldRot);
                }
            }
        }

        tileMovement.LastTickLocalCoordinates = targetTransform.LocalPosition;
        Dirty(uid, tileMovement);
        return true;
    }

    private bool CheckForSlideEnd(
        MoveButtons pressedButtons,
        TransformComponent transform,
        TileMovementComponent tileMovement,
        float movementSpeed
    )
    {
        // minPressedTime will be 1.05x the time it should take for you to go from 1 tile to another. Need to
        // account for diagonals being sqrt(2) length as well. Max of 10 seconds just in case.
        var distanceToDestination = (tileMovement.Destination - tileMovement.Origin.Position).Length();
        var minPressedTime = Math.Min((1.05f / movementSpeed) * distanceToDestination, 20);

        // We need to stop the move once we are close enough. This isn't perfect, since it technically ends the move
        // 1 tick early in some cases. This is because there's a fundamental issue where because this is a physics-based
        // tile movement system, we sometimes find scenarios where on each tick of the physics system, the player is moved
        // back and forth across the destination in a loop. Thus, the tolerance needs to be set overly high so that it
        // reaches the distance one the physics body can move in a single tick.
        float destinationTolerance = movementSpeed / 100f;

        var reachedDestination =
            transform.LocalPosition.EqualsApprox(tileMovement.Destination, destinationTolerance);
        var stoppedPressing = pressedButtons != tileMovement.CurrentSlideMoveButtons;
        var minDurationPassed = CurrentTime - tileMovement.MovementKeyInitialDownTime >= TimeSpan.FromSeconds(minPressedTime);
        var noProgress = tileMovement.LastTickLocalCoordinates != null && transform.LocalPosition.EqualsApprox(tileMovement.LastTickLocalCoordinates.Value, destinationTolerance/3);
        var hardDurationLimitPassed = CurrentTime - tileMovement.MovementKeyInitialDownTime >= TimeSpan.FromSeconds(minPressedTime) * 3;
        return reachedDestination || (stoppedPressing && (minDurationPassed || noProgress)) || hardDurationLimitPassed;
    }


    /// <summary>
    /// Initializes a slide, setting destination and other variables needed to start a slide to the given
    /// position (which is a local coordinate relative to the parent of the given uid).
    /// </summary>
    /// <param name="uid">UID of the entity that will be performing the slide.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity represented by UID.</param>
    /// <param name="localPositionTarget">Target of the slide coordinates local to the parent entity of uid.</param>
    /// <param name="heldMoveButtons">Buttons used to initiate this slide.</param>
    private void InitializeSlideToTarget(
        EntityUid uid,
        TileMovementComponent tileMovement,
        Vector2 localPositionTarget,
        MoveButtons heldMoveButtons)
    {
        var transform = Transform(uid);
        var localPosition = transform.LocalPosition;

        tileMovement.SlideActive = true;
        tileMovement.Origin = new EntityCoordinates(transform.ParentUid, localPosition);
        tileMovement.Destination = SnapCoordinatesToTile(localPositionTarget);
        tileMovement.MovementKeyInitialDownTime = CurrentTime;
        tileMovement.CurrentSlideMoveButtons = heldMoveButtons;
    }

    /// <summary>
    /// Initializes a slide, setting destination and other variables needed to start a slide to the center of the
    /// tile the entity is currently on.
    /// </summary>
    /// <param name="uid">UID of the entity that will be performing the slide.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity represented by UID.</param>
    private void InitializeSlideToCenter(EntityUid uid, TileMovementComponent tileMovement)
    {
        var localPosition = Transform(uid).LocalPosition;
        InitializeSlideToTarget(uid, tileMovement, SnapCoordinatesToTile(localPosition), MoveButtons.None);
    }

    /// <summary>
    /// Initializes a slide, setting destination and other variables needed to move in the direction currently given by
    /// the InputMoverComponent.
    /// </summary>
    /// <param name="uid">UID of the entity that will be performing the slide.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity represented by UID.</param>
    /// <param name="inputMover">InputMoverComponent on the entity represented by UID.</param>
    private void InitializeSlide(EntityUid uid, TileMovementComponent tileMovement, InputMoverComponent inputMover)
    {
        var transform = Transform(uid);
        var localPosition = transform.LocalPosition;
        var offset = DirVecForButtons(inputMover.HeldMoveButtons);
        offset = inputMover.TargetRelativeRotation.RotateVec(offset);

        InitializeSlideToTarget(uid, tileMovement, localPosition + offset, StripWalk(inputMover.HeldMoveButtons));
    }

    /// <summary>
    /// Updates the velocity of the current physics-based tile movement slide on the given entity.
    /// </summary>
    /// <param name="uid">UID of the entity being moved.</param>
    /// <param name="physicsUid">UID of the entity with the physics body being moved. Usually the same as uid.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity that's being moved.</param>
    /// <param name="inputMover">InputMoverComponent of the person controlling the move.</param>
    private void UpdateSlide(
        EntityUid uid,
        EntityUid physicsUid,
        TileMovementComponent tileMovement,
        InputMoverComponent inputMover
    )
    {
        var targetTransform = Transform(uid);

        if (PhysicsQuery.TryComp(physicsUid, out var physicsComponent))
        {
            // Gather some components and values.
            var moveSpeedComponent = ModifierQuery.CompOrNull(uid);
            var parentRotation = Angle.Zero;
            if (XformQuery.TryGetComponent(targetTransform.GridUid, out var relativeTransform))
            {
                parentRotation = _transform.GetWorldRotation(relativeTransform);
            }

            // Determine velocity based on movespeed, and rotate it so that it's in the right direction.
            var movementVelocity = (tileMovement.Destination) - (targetTransform.LocalPosition);
            movementVelocity.Normalize();
            if (inputMover.Sprinting)
            {
                movementVelocity *= moveSpeedComponent?.CurrentSprintSpeed ??
                    MovementSpeedModifierComponent.DefaultBaseSprintSpeed;
            }
            else
            {
                movementVelocity *= moveSpeedComponent?.CurrentWalkSpeed ??
                    MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
            }

            movementVelocity = parentRotation.RotateVec(movementVelocity);

            // Apply final velocity to physics body.
            PhysicsSystem.SetLinearVelocity(physicsUid, movementVelocity, body: physicsComponent);
            PhysicsSystem.SetAngularVelocity(physicsUid, 0, body: physicsComponent);
        }
    }

    /// <summary>
    /// Sets values on a TileMovementComponent designating that the slide has ended and sets it velocity to zero.
    /// </summary>
    /// <param name="uid">UID of the entity whose slide is being ended.</param>
    /// <param name="tileMovement">TileMovementComponent on the entity represented by UID.</param>
    private void EndSlide(EntityUid uid, TileMovementComponent tileMovement)
    {
        tileMovement.SlideActive = false;
        tileMovement.MovementKeyInitialDownTime = null;
        var physicsComponent = PhysicsQuery.GetComponent(uid);
        PhysicsSystem.SetLinearVelocity(uid, Vector2.Zero, body: physicsComponent);
        PhysicsSystem.SetAngularVelocity(uid, 0, body: physicsComponent);
    }

    /// <summary>
    /// Instantly snaps/teleports an entity to the center of the tile it is currently standing on based on the
    /// given grid. Does not trigger collisions on the way there, but does trigger collisions after the snap.
    /// </summary>
    /// <param name="uid">UID of entity to be snapped.</param>
    /// <param name="inputMover">InputMoverComponent on the entity to be snapped.</param>
    private void ForceSnapToTile(EntityUid uid, InputMoverComponent inputMover)
    {
        if (TryComp(inputMover.RelativeEntity, out TransformComponent? rel))
        {
            var targetTransform = Transform(uid);

            var localCoordinates = targetTransform.LocalPosition;
            var snappedCoordinates = SnapCoordinatesToTile(localCoordinates);

            if (!localCoordinates.EqualsApprox(snappedCoordinates) && targetTransform.ParentUid.IsValid())
            {
                _transform.SetLocalPosition(uid, snappedCoordinates);
            }

            PhysicsSystem.WakeBody(uid);
        }
    }

    /// <summary>
    /// Returns the movespeed of the given entity.
    /// </summary>
    /// <param name="uid">UID of the entity whose movespeed is being grabbed. May or may not have a MoveSpeedComponent.</param>
    /// <param name="sprinting">Whether the speed of the entity while sprinting should be grabbed.</param>
    /// <returns></returns>
    private float GetEntityMoveSpeed(EntityUid uid, bool sprinting)
    {
        var moveSpeedComponent = ModifierQuery.CompOrNull(uid);
        if (sprinting)
        {
            return moveSpeedComponent?.CurrentSprintSpeed ?? MovementSpeedModifierComponent.DefaultBaseSprintSpeed;
        }

        return moveSpeedComponent?.CurrentWalkSpeed ?? MovementSpeedModifierComponent.DefaultBaseWalkSpeed;
    }

    private float GetEntityFriction(
        InputMoverComponent inputMover,
        MovementSpeedModifierComponent? movementSpeedComponent,
        ContentTileDefinition? tileDef
    )
    {
        if (inputMover.HeldMoveButtons != MoveButtons.None || movementSpeedComponent?.FrictionNoInput == null)
        {
            return tileDef?.MobFriction ??
                movementSpeedComponent?.Friction ?? MovementSpeedModifierComponent.DefaultFriction;
        }

        return tileDef?.MobFrictionNoInput ?? movementSpeedComponent.FrictionNoInput ??
            MovementSpeedModifierComponent.DefaultFrictionNoInput;
    }

    /// <summary>
    /// Sets the walk value on the given MoveButtons input to zero.
    /// </summary>
    /// <param name="input">The MoveButtons to edit.</param>
    /// <returns></returns>
    private MoveButtons StripWalk(MoveButtons input)
    {
        return input & ~MoveButtons.Walk;
    }

    /// <summary>
    /// Returns the given local coordinates snapped to the center of the tile it is currently on.
    /// </summary>
    /// <param name="input">Given coordinates to snap.</param>
    /// <returns>The closest tile center to the input.<returns>
    public static Vector2 SnapCoordinatesToTile(Vector2 input)
    {
        return new Vector2((int) Math.Floor(input.X) + 0.5f, (int) Math.Floor(input.Y) + 0.5f);
    }
    // Tile Movement Functions End
}
