using System.Numerics;
using Content.Shared._Goobstation.PrisonerId;
using Content.Shared.Access.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Mobs.Components;
using Content.Shared.PDA;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
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
    }

    //really need to think about this
    private void OnPreventCollide(EntityUid uid, TurnstileComponent comp, ref PreventCollideEvent args)
    {
        if (comp.PassingThrough == null)
            return;

        args.Cancelled = comp.PassingThrough == args.OtherEntity;
    }
    private void DisallowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        _audioSystem.PlayPredicted(comp.DenySound, uid, otherEntity);
        _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Deny);
        Timer.Spawn(1000,
            () =>
            {
                _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Base);
            });
    }

    private void AllowedPassage(EntityUid uid, TurnstileComponent comp, EntityUid otherEntity)
    {
        // Check access here.
        _audioSystem.PlayPredicted(comp.AccessSound, uid, otherEntity);
        _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Allow);

        // Allowed passage
        comp.PassingThrough = otherEntity;
        Timer.Spawn(1000,
            () =>
            {
                StartPrisonerTime(otherEntity);
                comp.PassingThrough = null;
                _appearanceSystem.SetData(uid, TurnstileVisuals.State, TurnstileVisualState.Base);
            });
    }

    private void StartPrisonerTime(EntityUid ent)
    {
        var id = FindId(ent);
        if (id == default)
            return;

        // tell the id to start counting :)
        RaiseNetworkEvent(new StartPrisonerSentence(GetNetEntity(id)));
    }


    private EntityUid FindId(EntityUid ent)
    {
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
        if (!HasComp<MobStateComponent>(args.OtherEntity))
            return;

        if (!_accessReaderSystem.IsAllowed(args.OtherEntity, uid))
        {
            DisallowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        var approachDir = _transformSystem.GetWorldRotation(args.OtherEntity).GetCardinalDir();
        var turnstileDir = _transformSystem.GetWorldRotation(uid).GetCardinalDir();

        if (approachDir == turnstileDir)
        {
            AllowedPassage(uid, comp, args.OtherEntity);
            return;
        }

        DisallowedPassage(uid, comp, args.OtherEntity);
    }
}
