// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Power.Components;

namespace Content.Shared.Power.EntitySystems;

public sealed class ItemSlotRequiresPowerSystem : EntitySystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _receiver = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSlotRequiresPowerComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt);
    }

    private void OnInsertAttempt(Entity<ItemSlotRequiresPowerComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (!_receiver.IsPowered(ent.Owner))
        {
            args.Cancelled = true;
        }
    }
}