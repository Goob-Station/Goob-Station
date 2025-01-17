using System.Numerics;
using Content.Shared._Goobstation.PrisonerId;
using Content.Shared.Access;
using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
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
    [Dependency] protected readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] protected readonly SharedAccessSystem _accessSystem = default!;
    [Dependency] protected readonly InventorySystem _inventorySystem = default!;
    [Dependency] protected readonly SharedHandsSystem _handsSystem = default!;



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

    private void DisallowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        _audioSystem.PlayPredicted(comp.DenySound,uid, otherEntity);
        RaiseNetworkEvent(new BadTurnstileEvent(GetNetEntity(uid)));
    }

    private void AllowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        // Check access here.

        _audioSystem.PlayPredicted(comp.AccessSound,uid, otherEntity);
        RaiseNetworkEvent(new StartTurnstileEvent(GetNetEntity(uid)));

        // Allowed passage
        _physicsSystem.SetCanCollide(uid, false);
        Timer.Spawn(500,
            () =>
            {
                StartPrisonerTime(otherEntity);
                _physicsSystem.SetCanCollide(uid, true);
            });
    }

    private void StartPrisonerTime(EntityUid ent)
    {
        Logger.Debug("firing event");
        var id = FindId(ent);
        if (id == EntityUid.Invalid)
        {
            Logger.Debug("Didnt find prisoner ID");
            return;
        }

        // tell the id to start counting :)
        RaiseNetworkEvent(new StartPrisonerSentence(GetNetEntity(id)));
    }

    private EntityUid FindId(EntityUid ent)
    {
        var slotEnumerator = _inventorySystem.GetSlotEnumerator(ent);
        using var handsEnumerator = _handsSystem.EnumerateHands(ent).GetEnumerator();
        while (handsEnumerator.MoveNext())
        {
            var hand = handsEnumerator.Current;
            if (hand.Container == null)
                continue;

            var uid = hand.Container.ContainedEntity;
            if (!TryComp<MetaDataComponent>(uid, out var metaData))
                continue;

            if (metaData?.EntityPrototype?.ID == "PrisonerID") // unhardcode
                return (EntityUid) uid!;
        }

        while (slotEnumerator.MoveNext(out var slot))
        {
            var slotEntity = slot.ContainedEntity;
            if (!TryComp<PdaComponent>(slotEntity, out var pdaComponent))
            {
                continue;
            }

            if (!TryComp<MetaDataComponent>(pdaComponent.ContainedId, out var pdaIdMetaData))
                continue;

            if (pdaIdMetaData?.EntityPrototype?.ID == "PrisonerID") // unhardcode
                return (EntityUid) pdaComponent.ContainedId!;
        }

        return EntityUid.Invalid;
    }

    private void OnStartCollide(EntityUid uid, TurnstileComponent comp, StartCollideEvent args)
    {
        if (!_accessReaderSystem.IsAllowed(args.OtherEntity, uid))
        {
            DisallowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        var approachVector =  _transformSystem.GetWorldPosition(args.OtherEntity) - _transformSystem.GetWorldPosition(uid);
        var normalizedApproach = approachVector.Normalized();

        if (Vector2.Dot(normalizedApproach, comp.AllowedDirection) > 0.5f)
        {
            AllowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        DisallowedPassage(uid, comp, args.OtherEntity);
    }
}
