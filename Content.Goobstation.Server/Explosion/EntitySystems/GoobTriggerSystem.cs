// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Solstice <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Explosion.Components;
using Content.Goobstation.Server.Explosion.Components.OnTrigger;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
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


        foreach (var hand in _hands.EnumerateHands(containingEntity.Value, hands))
        {
            if (hand.HeldEntity == null)
                continue;

            _hands.TryDrop(containingEntity.Value, hand, handsComp: hands);
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
