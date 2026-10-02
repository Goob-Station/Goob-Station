// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.BlockHandsOnBuckle;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Buckle.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Interaction.Events;

namespace Content.Goobstation.Shared.BlockHandsOnBuckle;

public sealed class BlockHandsOnBuckleSystem : EntitySystem
{
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlockHandsOnBuckleComponent, StrappedEvent>(OnBuckled);
        SubscribeLocalEvent<BlockHandsOnBuckleComponent, UnstrappedEvent>(OnUnstrapped);

        SubscribeLocalEvent<BuckleComponent, AttackAttemptEvent>(OnCanAttack);
        SubscribeLocalEvent<BuckleComponent, InteractionAttemptEvent>(OnInteractionAttempt);
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

    private void OnInteractionAttempt(Entity<BuckleComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.BuckledTo is { } buckled
            && HasComp<BlockHandsOnBuckleComponent>(buckled)
            && args.Target != null)
            args.Cancelled = true;
    }

    private void OnCanAttack(Entity<BuckleComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.BuckledTo is { } buckled
            && HasComp<BlockHandsOnBuckleComponent>(buckled))
            args.Cancel();
    }
}
