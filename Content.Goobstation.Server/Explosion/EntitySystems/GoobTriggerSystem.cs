// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Explosion.Components;
using Content.Goobstation.Server.Explosion.Components.OnTrigger;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Systems;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed class GoobTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
        SubscribeLocalEvent<DropOnTriggerComponent, TriggerEvent>(HandleDropOnTrigger);
        SubscribeLocalEvent<TriggerOnMeleeComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void HandleDeleteParentTrigger(Entity<DeleteParentOnTriggerComponent> entity, ref TriggerEvent args)
    {
        EntityManager.QueueDeleteEntity(Transform(entity).ParentUid); // cleanedup - goob mudles
        args.Handled = true;
    }

    private void HandleDropOnTrigger(Entity<DropOnTriggerComponent> entity, ref TriggerEvent args)
    {
        if (!TryComp(entity, out HandsComponent? hands) || !_inventory.TryGetContainingEntity(entity.Owner, out var containingEntity))
            return;


        foreach (var hand in _hands.EnumerateHands((containingEntity.Value, hands)))
        {
            if (_hands.GetHeldItem((containingEntity.Value, hands), hand) == null)
                continue;

            _hands.TryDrop((containingEntity.Value, hands), hand);
        }
        args.Handled = true;
    }

    private void OnMeleeHit(Entity<TriggerOnMeleeComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit
            || args.HitEntities.Count <= 0)
            return;

        _trigger.Trigger(ent, ent);
    }
}
