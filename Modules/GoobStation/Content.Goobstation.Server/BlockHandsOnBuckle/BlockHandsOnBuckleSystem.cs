// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 TheBorzoiMustConsume <197824988+TheBorzoiMustConsume@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.BlockHandsOnBuckle;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Buckle.Components;
using Content.Shared.Hands;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Interaction.Events;

namespace Content.Goobstation.Server.BlockHandsOnBuckle;
public sealed class BlockHandsOnBuckleSystem : EntitySystem
{

    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlockHandsOnBuckleComponent, StrappedEvent>(OnBuckled);
        SubscribeLocalEvent<BlockHandsOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);

        SubscribeLocalEvent<BuckleComponent, AttackAttemptEvent>(OnCanAttack);
    }

    private void OnBuckled(Entity<BlockHandsOnBuckleComponent> ent, ref StrappedEvent args)
    {
        var victim = args.Buckle.Owner;
        foreach (var hand in _handsSystem.EnumerateHands(victim))
        {
            _handsSystem.TryDrop(victim, hand);
            _virtualItem.TrySpawnVirtualItemInHand(ent.Owner, victim, true);
            if (_handsSystem.TryGetHeldItem(victim, hand, out var held) && held != null)
            {
                EnsureComp<UnremoveableComponent>(held.Value);
            }
        }
    }
    
    private void OnUnstrapped(Entity<BlockHandsOnBuckleComponent> ent, ref UnstrappedEvent args)
    {
        _virtualItem.DeleteInHandsMatching(args.Buckle.Owner, ent.Owner);

    }
    
    private void OnCanAttack(EntityUid uid, BuckleComponent buckle, ref AttackAttemptEvent args)
    {
        if (buckle.BuckledTo != null
            && HasComp<BlockHandsOnBuckleComponent>(buckle.BuckledTo.Value))
            args.Cancel();
    }
}
