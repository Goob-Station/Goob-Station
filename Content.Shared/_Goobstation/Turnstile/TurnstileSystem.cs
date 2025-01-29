using System.Numerics;
using Content.Shared._Goobstation.PrisonerId;
using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Turnstile;

/// <summary>
///     This handles the turnstiles logic.
/// </summary>
public sealed class TurnstileSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;


    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurnstileComponent, StartCollideEvent>(OnStartCollide);
        SubscribeLocalEvent<TurnstileComponent, PreventCollideEvent>(OnPreventCollide);
        SubscribeLocalEvent<TurnstileComponent, ComponentStartup>(OnComponentStartup);
    }

    //really need to think about this
    private void OnPreventCollide(EntityUid uid, TurnstileComponent comp, ref PreventCollideEvent args)
    {
        if (comp.PassingThrough == null)
        {
            comp.PassingThrough = args.OtherEntity;
            args.Cancelled = false; // Allow collision
        }

        if (comp.PassingThrough != args.OtherEntity)
            args.Cancelled = true;

        args.Cancelled = false;
    }

    private void OnComponentStartup(EntityUid uid, TurnstileComponent comp, ref ComponentStartup args)
    {
        var directionVector = GetStructureDirectionVector(uid);
        comp.AllowedDirection = directionVector;
    }


    public Vector2 GetStructureDirectionVector(EntityUid entityUid)
    {
        var worldRotation = _transformSystem.GetWorldRotation(entityUid);

        return GetDirectionVectorFromRotation(worldRotation);
    }

    private Vector2 GetDirectionVectorFromRotation(Angle rotation)
    {
        var cardinal = rotation.GetCardinalDir();
        switch (cardinal)
        {
            case Direction.North:
                return new Vector2(0, 1);
            case Direction.East:
                return new Vector2(1, 0);
            case Direction.South:
                return new Vector2(0, -1);
            case Direction.West:
                return new Vector2(-1, 0);
            case Direction.NorthEast:
                return new Vector2(0.707f, 0.707f);
            case Direction.SouthEast:
                return new Vector2(0.707f, 0.707f);
            case Direction.NorthWest:
                return new Vector2(-0.707f, 0.707f);
            case Direction.SouthWest:
                return new Vector2(-0.707f, -0.707f);

            default:
                return new Vector2(0, 1); // North
        }
    }

    private void DisallowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        _audioSystem.PlayPredicted(comp.DenySound, uid, otherEntity);
        _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Deny);
    }

    private void AllowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        // Check access here.
        _audioSystem.PlayPredicted(comp.AccessSound, uid, otherEntity);
        _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Allow);

        // Allowed passage
        comp.PassingThrough = otherEntity;
        _physicsSystem.SetCanCollide(uid, false);
        Timer.Spawn(1000,
            () =>
            {
                StartPrisonerTime(otherEntity, comp);
                _physicsSystem.SetCanCollide(uid, true);
                comp.PassingThrough = null;
            });
    }

    private void StartPrisonerTime(EntityUid ent, TurnstileComponent comp)
    {
        var id = FindId(ent, comp);
        if (id == default)
            return;

        // tell the id to start counting :)
        RaiseNetworkEvent(new StartPrisonerSentence(GetNetEntity(id)));
    }


    private EntityUid FindId(EntityUid ent, TurnstileComponent comp)
    {
        if (!TryComp<InventoryComponent>(ent, out var inventoryComponent))
            return default;

        var inventoryEntities = _inventorySystem.GetHandOrInventoryEntities(ent);
        var heldEntities = _handsSystem.EnumerateHeld(ent);

        foreach (var held in heldEntities)
        {
            if (_tagSystem.HasTag(held, "PrisonerIdCard"))
                return held;
        }

        foreach (var inventoryEntity in inventoryEntities)
        {
            if (!TryComp<PdaComponent>(inventoryEntity, out var pdaComponent))
                continue;

            // tag checking
            if (_tagSystem.HasTag(pdaComponent.ContainedId ?? default, "PrisonerIdCard"))
                return pdaComponent.ContainedId ?? default;
        }

        return default;
    }

    private void OnStartCollide(EntityUid uid, TurnstileComponent comp, StartCollideEvent args)
    {
        if (!_accessReaderSystem.IsAllowed(args.OtherEntity, uid))
        {
            DisallowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        var approachVector = _transformSystem.GetWorldPosition(args.OtherEntity) -
                             _transformSystem.GetWorldPosition(uid);
        var normalizedApproach = approachVector.Normalized();

        if (Vector2.Dot(normalizedApproach, comp.AllowedDirection) > 0.5f)
        {
            AllowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        DisallowedPassage(uid, comp, args.OtherEntity);
    }
}
