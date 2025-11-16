// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <92227810+SX-7@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Penalties.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Goobstation.Shared.Penalties.Systems;

public sealed partial class OuterSlotPenaltySystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _clothingSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OuterSlotPenaltyComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<OuterSlotPenaltyComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<OuterSlotPenaltyComponent, RefreshMovementSpeedModifiersEvent>(OnMove);
    }

    private void OnEntInserted(EntityUid uid, OuterSlotPenaltyComponent comp, ref EntInsertedIntoContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth) && cloth.Slots == SlotFlags.OUTERCLOTHING)
        {
            cloth.EquipDelay = TimeSpan.FromSeconds(comp.EquipDelay);
            cloth.UnequipDelay = TimeSpan.FromSeconds(comp.UnequipDelay);
        }
    }

    private void OnEntRemoved(EntityUid uid, OuterSlotPenaltyComponent comp, ref EntRemovedFromContainerMessage args)
    {
        if (TryComp<ClothingComponent>(args.Entity, out var cloth) && cloth.Slots == SlotFlags.OUTERCLOTHING)
        {
            cloth.EquipDelay = default!;
            cloth.UnequipDelay = default!;
        }
    }

    private void OnMove(EntityUid uid, OuterSlotPenaltyComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (comp.OuterLayerEquipped)
            args.ModifySpeed(comp.EquippedSpeedMultiplier);
    }
}