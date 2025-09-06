using System.Numerics;
using Content.Shared.Interaction;
using Content.Server.Shuttles.Components;
using Content.Shared.Projectiles;
using Robust.Server.GameObjects;

namespace Content.Server._Mono.Projectiles.TargetSeeking;

/// <summary>
/// Handles the logic for target-seeking projectiles.
/// </summary>
public sealed class TargetSeekingSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly RotateToFaceSystem _rotateToFace = null!;
    [Dependency] private readonly PhysicsSystem _physics = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetSeekingComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    /// <summary>
    /// Called when a target-seeking projectile hits something.
    /// </summary>
    private void OnProjectileHit(EntityUid uid, TargetSeekingComponent component, ref ProjectileHitEvent args)
    {
        // If we hit our actual target, we could perform additional effects here
        if (component.CurrentTarget.HasValue && component.CurrentTarget.Value == args.Target)
        {
            // Target hit successfully
        }

        // Reset the target since we've hit something
        component.CurrentTarget = null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TargetSeekingComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var seekingComp, out var xform))
        {
            // Initialize speed if needed
            if (seekingComp.CurrentSpeed < seekingComp.LaunchSpeed)
            {
                seekingComp.CurrentSpeed = seekingComp.LaunchSpeed;
            }

            // Accelerate up to max speed
            if (seekingComp.CurrentSpeed < seekingComp.MaxSpeed)
            {
                seekingComp.CurrentSpeed += seekingComp.Acceleration * frameTime;
            }
            else
            {
                seekingComp.CurrentSpeed = seekingComp.MaxSpeed;
            }

            // Apply velocity in the direction the projectile is facing
            _physics.SetLinearVelocity(uid, _transform.GetWorldRotation(xform).ToWorldVec() * seekingComp.CurrentSpeed);

            // If we have a target, track it using the selected algorithm
            if (seekingComp.CurrentTarget.HasValue)
            {
                if (seekingComp.TrackingAlgorithm == TrackingMethod.Predictive)
                {
                    ApplyPredictiveTracking(uid, seekingComp, xform, frameTime);
                }
                else
                {
                    ApplyDirectTracking(uid, seekingComp, xform, frameTime);
                }
            }
            else
            {
                // Try to acquire a new target
                AcquireTarget(uid, seekingComp, xform);
            }
        }
    }

    /// <summary>
    /// Finds the closest valid target within range and tracking parameters.
    /// </summary>
    public void AcquireTarget(EntityUid uid, TargetSeekingComponent component, TransformComponent transform)
    {
        var closestDistance = float.MaxValue;
        EntityUid? bestTarget = null;

        // Look for shuttles to target
        var shuttleQuery = EntityQueryEnumerator<ShuttleConsoleComponent, TransformComponent>();

        while (shuttleQuery.MoveNext(out var targetUid, out _, out var targetXform))
        {
            // If this entity has a grid UID, use that as our actual target
            // This targets the ship grid rather than just the console
            var actualTarget = targetXform.GridUid ?? targetUid;

            // Get angle to the target
            var targetPos = _transform.ToMapCoordinates(targetXform.Coordinates).Position;
            var sourcePos = _transform.ToMapCoordinates(transform.Coordinates).Position;
            var angleToTarget = (targetPos - sourcePos).ToWorldAngle();

            // Get current direction of the projectile
            var currentRotation = _transform.GetWorldRotation(transform);

            // Check if target is within field of view
            var angleDifference = Angle.ShortestDistance(currentRotation, angleToTarget).Degrees;
            if (MathF.Abs((float)angleDifference) > component.FieldOfView / 2)
            {
                continue; // Target is outside our field of view
            }

            // Calculate distance to target
            var distance = Vector2.Distance(sourcePos, targetPos);

            // Skip if target is out of range
            if (distance > component.DetectionRange)
            {
                continue;
            }

            // Skip if the target is our own launcher (don't target our own ship)
            if (TryComp<ProjectileComponent>(uid, out var projectile) &&
                TryComp<TransformComponent>(projectile.Shooter, out var shooterTransform))
            {
                var shooterGridUid = shooterTransform.GridUid;

                // If the shooter is on the same grid as this potential target, skip it
                if (targetXform.GridUid.HasValue && shooterGridUid == targetXform.GridUid)
                {
                    continue;
                }
            }

            // If this is closer than our previous best target, update
            if (closestDistance > distance)
            {
                closestDistance = distance;
                bestTarget = actualTarget;
            }
        }

        // Set our new target
        if (bestTarget.HasValue)
        {
            component.CurrentTarget = bestTarget;

            // Initialize tracking data
            if (TryComp<TransformComponent>(bestTarget, out var targetXform))
            {
                component.PreviousTargetPosition = _transform.ToMapCoordinates(targetXform.Coordinates).Position;
                component.PreviousDistance = closestDistance;
            }
        }
    }

    /// <summary>
    /// Advanced tracking that predicts where the target will be based on its velocity.
    /// </summary>
    public void ApplyPredictiveTracking(EntityUid uid, TargetSeekingComponent comp, TransformComponent xform, float frameTime)
    {
        if (!comp.CurrentTarget.HasValue || !TryComp<TransformComponent>(comp.CurrentTarget.Value, out var targetXform))
        {
            return;
        }

        // Get current positions
        var currentTargetPosition = _transform.ToMapCoordinates(targetXform.Coordinates).Position;
        var sourcePosition = _transform.ToMapCoordinates(xform.Coordinates).Position;

        // Calculate current distance
        var currentDistance = Vector2.Distance(sourcePosition, currentTargetPosition);

        // Calculate target velocity
        var targetVelocity = currentTargetPosition - comp.PreviousTargetPosition;

        // Calculate time to intercept (using closing rate)
        var closingRate = (comp.PreviousDistance - currentDistance);
        var timeToIntercept = closingRate > 0.01f ? currentDistance / closingRate : currentDistance / comp.CurrentSpeed;

        // Prevent negative or very small intercept times that could cause erratic behavior
        timeToIntercept = MathF.Max(timeToIntercept, 0.1f);

        // Predict where the target will be when we reach it
        var predictedPosition = currentTargetPosition + (targetVelocity * timeToIntercept);

        // Calculate angle to the predicted position
        var targetAngle = (predictedPosition - sourcePosition).ToWorldAngle();

        // Rotate toward that angle at our turn rate
        _rotateToFace.TryRotateTo(
            uid,
            targetAngle,
            frameTime,
            comp.ScanArc,
            comp.TurnRate?.Theta ?? MathF.PI * 2,
            xform
        );

        // Update tracking data for next frame
        comp.PreviousTargetPosition = currentTargetPosition;
        comp.PreviousDistance = currentDistance;
    }

    /// <summary>
    /// Basic tracking that points directly at the current target position.
    /// </summary>
    public void ApplyDirectTracking(EntityUid uid, TargetSeekingComponent comp, TransformComponent xform, float frameTime)
    {
        if (!comp.CurrentTarget.HasValue || !TryComp<TransformComponent>(comp.CurrentTarget.Value, out var targetXform))
        {
            return;
        }

        // Get the angle directly toward the target
        var angleToTarget = (
            _transform.ToMapCoordinates(targetXform.Coordinates).Position -
            _transform.ToMapCoordinates(xform.Coordinates).Position
        ).ToWorldAngle();

        // Rotate toward that angle at our turn rate
        _rotateToFace.TryRotateTo(
            uid,
            angleToTarget,
            frameTime,
            comp.ScanArc,
            comp.TurnRate?.Theta ?? MathF.PI * 2,
            xform
        );
    }
}
