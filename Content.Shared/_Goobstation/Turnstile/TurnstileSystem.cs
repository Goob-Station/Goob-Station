using System.Numerics;
using Content.Shared.Access.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Turnstile;

/// <summary>
/// This handles...
/// </summary>
public sealed class TurnstileSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] protected readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] protected readonly AccessReaderSystem _accessSystem = default!;



    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<_Goobstation.Turnstile.TurnstileComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<_Goobstation.Turnstile.TurnstileComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, _Goobstation.Turnstile.TurnstileComponent component, ref ComponentStartup args)
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

    private void DisallowedPassage(EntityUid uid, _Goobstation.Turnstile.TurnstileComponent comp, EntityUid otherEntity)
    {
        _audioSystem.PlayPredicted(comp.DenySound,uid, otherEntity);
        RaiseNetworkEvent(new BadTurnstileEvent(GetNetEntity(uid)));
    }

    private void AllowedPassage(EntityUid uid, _Goobstation.Turnstile.TurnstileComponent comp, EntityUid otherEntity)
    {
        // Check access here.

        _audioSystem.PlayPredicted(comp.AccessSound,uid, otherEntity);
        RaiseNetworkEvent(new StartTurnstileEvent(GetNetEntity(uid)));

        // Allowed passage
        _physicsSystem.SetCanCollide(uid, false);
        Timer.Spawn(500,
            () =>
            {
                _physicsSystem.SetCanCollide(uid, true);
            });
    }

    private void OnStartCollide(EntityUid uid, _Goobstation.Turnstile.TurnstileComponent comp, StartCollideEvent args)
    {
        if (!_accessSystem.IsAllowed(args.OtherEntity, uid))
        {
            DisallowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        var approachVector =  _transformSystem.GetWorldPosition(args.OtherEntity) - _transformSystem.GetWorldPosition(uid);
        var normalizedApproach = approachVector.Normalized();

        if (Vector2.Dot(normalizedApproach, comp.AllowedDirection) > 0.5f)
        {
            AllowedPassage(uid, comp, args.OtherEntity);
        }
        else
        {
            DisallowedPassage(uid, comp, args.OtherEntity);
        }
    }
}


