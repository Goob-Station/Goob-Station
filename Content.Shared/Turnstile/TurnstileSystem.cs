using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Turnstile;

/// <summary>
/// This handles...
/// </summary>
public sealed class TurnstileSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;



    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurnstileComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<TurnstileComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, TurnstileComponent component, ref ComponentStartup args)
    {
        var dir = GetStructureDirection(uid);
        component.AllowedDirection = dir;
    }


    public Vector2 GetStructureDirection(EntityUid entityUid)
    {
        // Get the world rotation of the entity
        var worldRotation = _transformSystem.GetWorldRotation(entityUid);

        // Convert the rotation to one of the four cardinal directions
        return GetDirectionFromRotation(worldRotation);
    }

    private Vector2 GetDirectionFromRotation(Angle rotation)
    {
        var degrees = rotation.Degrees % 360; // Ensure the angle is within 0-360
        if (degrees < 0)
            degrees += 360;

        if (degrees >= 45 && degrees < 135)
            return new Vector2(-1,0); // West
        if (degrees >= 135 && degrees < 225)
            return new Vector2(0,-1); // East
        if (degrees >= 225 && degrees < 315)
            return new Vector2(1,0);

        return new Vector2(0,1);
    }

    private void OnStartCollide(EntityUid uid, TurnstileComponent component, StartCollideEvent args)
    {
        // Calculate the approach vector
        var approachVector =  _transformSystem.GetWorldPosition(args.OtherEntity) - _transformSystem.GetWorldPosition(uid);

        // Normalize the approach vector
        var normalizedApproach = approachVector.Normalized();

        // Check if the approach vector aligns with the allowed direction
        if (Vector2.Dot(normalizedApproach, component.AllowedDirection) > 0.5f)
        {


            RaiseNetworkEvent(new StartTurnstileEvent(GetNetEntity(uid)));

            _audioSystem.PlayPredicted(component.AccessSound,uid, args.OtherEntity);
            // Allow passage (e.g., disable collision temporarily)
            _physicsSystem.SetCanCollide(uid, false);
            // Optionally re-enable collision after a delay
            Timer.Spawn(500,
                () =>
                {
                    _physicsSystem.SetCanCollide(uid, true);
                });
        }
        else
        {
            _audioSystem.PlayPredicted(component.DenySound,uid, args.OtherEntity);
           RaiseNetworkEvent(new BadTurnstileEvent(GetNetEntity(uid)));

        }
    }
}


