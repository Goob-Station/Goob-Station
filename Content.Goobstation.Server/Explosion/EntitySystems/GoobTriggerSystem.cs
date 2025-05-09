// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Explosion.Components;
using Content.Goobstation.Server.Explosion.Components.OnTrigger;
using Content.Server._Goobstation.Explosion.Components;
using Content.Server.Explosion.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Goobstation.Server.Explosion.EntitySystems;

public sealed partial class GoobTriggerSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TriggerSystem _trigger = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    public override void Initialize()
    {
        base.Initialize();
        InitializeMelee();
        SubscribeLocalEvent<DeleteParentOnTriggerComponent, TriggerEvent>(HandleDeleteParentTrigger);
        SubscribeLocalEvent<DropOnTriggerComponent, TriggerEvent>(HandleDropOnTrigger);
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
}
