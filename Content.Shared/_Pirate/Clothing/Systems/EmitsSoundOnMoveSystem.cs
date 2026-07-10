// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Pirate.Clothing.Components;
using Content.Shared._Pirate.Clothing.Events;
using Content.Shared.Clothing.Components;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Shared._Pirate.Clothing.Systems;

public sealed class EmitsSoundOnMoveSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;
    private EntityQuery<ClothingComponent> _clothingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
        _clothingQuery = GetEntityQuery<ClothingComponent>();

        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<EmitsSoundOnMoveComponent, InventoryRelayedEvent<PirateMakeFootstepSoundEvent>>(OnFootstep);
    }

    private void OnEquipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotEquippedEvent args)
    {
        component.IsSlotValid = !args.SlotFlags.HasFlag(SlotFlags.POCKET);
    }

    private void OnUnequipped(EntityUid uid, EmitsSoundOnMoveComponent component, GotUnequippedEvent args)
    {
        component.IsSlotValid = true;
    }

    private void OnFootstep(Entity<EmitsSoundOnMoveComponent> ent, ref InventoryRelayedEvent<PirateMakeFootstepSoundEvent> args)
    {
        var uid = ent.Owner;
        var component = ent.Comp;

        if (!_physicsQuery.TryGetComponent(uid, out var physics)
            || !_timing.IsFirstTimePredicted)
            return;

        var xform = Transform(uid);
        if (xform.GridUid is null)
            return;

        if (component.RequiresGravity && _gravity.IsWeightless(uid))
            return;

        var parent = xform.ParentUid;
        var isWorn = parent is { Valid: true } &&
                     _clothingQuery.TryGetComponent(uid, out var clothing) &&
                     clothing.InSlot != null &&
                     component.IsSlotValid;

        if (component.RequiresWorn && !isWorn)
            return;

        var sound = component.SoundCollection;
        var audioParams = sound.Params
            .WithVolume(sound.Params.Volume)
            .WithVariation(sound.Params.Variation ?? 0f);

        _audio.PlayPredicted(sound, uid, uid, audioParams);
    }
}
