// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Luiz Costa <33888056+luizwritescode@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 themias <89101928+themias@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marty <martynashagriefer@gmail.com>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 paige404 <59348003+paige404@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Foldable;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Item;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class FoldableClothingSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FoldableClothingComponent, MapInitEvent>(OnMapInit); // Goob - #3632
        SubscribeLocalEvent<FoldableClothingComponent, FoldAttemptEvent>(OnFoldAttempt);
        SubscribeLocalEvent<FoldableClothingComponent, FoldedEvent>(OnFolded,
            after: [typeof(MaskSystem)]); // Mask system also modifies clothing / equipment RSI state prefixes.
    }

    // Goobstation Start - #3632
    private void OnMapInit(Entity<FoldableClothingComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.FoldedHideLayers.Count == 0 && ent.Comp.UnfoldedHideLayers.Count == 0)
            return;
        var hideLayer = EnsureComp<HideLayerClothingComponent>(ent.Owner);
        var slot = ent.Comp.UnfoldedSlots ?? ent.Comp.FoldedSlots ?? SlotFlags.NONE;

        foreach (var layer in ent.Comp.UnfoldedHideLayers)
            hideLayer.Layers[layer] = slot;

        Dirty(ent.Owner, hideLayer);
    }
    // Goobstation end
    private void OnFoldAttempt(Entity<FoldableClothingComponent> ent, ref FoldAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_inventorySystem.TryGetContainingSlot(ent.Owner, out var slot))
            return;

        // Cannot fold clothing equipped to a slot if the slot becomes disallowed
        var newSlots = args.Comp.IsFolded ? ent.Comp.UnfoldedSlots : ent.Comp.FoldedSlots;
        if (newSlots != null && (newSlots.Value & slot.SlotFlags) != slot.SlotFlags)
        {
            args.Cancelled = true;
            return;
        }

        // Setting hidden layers while equipped is not currently supported.
        if (ent.Comp.FoldedHideLayers.Count != 0|| ent.Comp.UnfoldedHideLayers.Count != 0)
            args.Cancelled = true;
    }

    private void OnFolded(Entity<FoldableClothingComponent> ent, ref FoldedEvent args)
    {
        if (!TryComp<ClothingComponent>(ent.Owner, out var clothingComp) ||
            !TryComp<ItemComponent>(ent.Owner, out var itemComp))
            return;

        if (args.IsFolded)
        {
            if (ent.Comp.FoldedSlots.HasValue)
                _clothingSystem.SetSlots(ent.Owner, ent.Comp.FoldedSlots.Value, clothingComp);

            if (ent.Comp.FoldedEquippedPrefix != null)
                _clothingSystem.SetEquippedPrefix(ent.Owner, ent.Comp.FoldedEquippedPrefix, clothingComp);

            if (ent.Comp.FoldedHeldPrefix != null)
                _itemSystem.SetHeldPrefix(ent.Owner, ent.Comp.FoldedHeldPrefix, false, itemComp);

            // This is janky and likely to lead to bugs.
            // I.e., overriding this and resetting it again later will lead to bugs if someone tries to modify clothing
            // in yaml, but doesn't realise theres actually two other fields on an unrelated component that they also need
            // to modify.
            // This should instead work via an event or something that gets raised to optionally modify the currently hidden layers.
            // Or at the very least it should stash the old layers and restore them when unfolded.
            // TODO CLOTHING fix this.
            if ((ent.Comp.FoldedHideLayers.Count != 0 || ent.Comp.UnfoldedHideLayers.Count != 0) &&
                TryComp<HideLayerClothingComponent>(ent.Owner, out var hideLayerComp))
                // Goobstation Start
            {
                foreach (var layer in ent.Comp.UnfoldedHideLayers)
                    hideLayerComp.Layers.Remove(layer);

                var slot = ent.Comp.FoldedSlots ?? ent.Comp.UnfoldedSlots ?? SlotFlags.NONE;

                foreach (var layer in ent.Comp.FoldedHideLayers)
                    hideLayerComp.Layers[layer] = slot;
                Dirty(ent.Owner, hideLayerComp);
            }
                // Goobstation End
        }
        else
        {
            if (ent.Comp.UnfoldedSlots.HasValue)
                _clothingSystem.SetSlots(ent.Owner, ent.Comp.UnfoldedSlots.Value, clothingComp);

            if (ent.Comp.FoldedEquippedPrefix != null)
                _clothingSystem.SetEquippedPrefix(ent.Owner, null, clothingComp);

            if (ent.Comp.FoldedHeldPrefix != null)
                _itemSystem.SetHeldPrefix(ent.Owner, null, false, itemComp);

            // TODO CLOTHING fix this.
            if ((ent.Comp.FoldedHideLayers.Count != 0 || ent.Comp.UnfoldedHideLayers.Count != 0) &&
                TryComp<HideLayerClothingComponent>(ent.Owner, out var hideLayerComp))
                // Goobstation Start
            {
                foreach (var layer in ent.Comp.FoldedHideLayers)
                    hideLayerComp.Layers.Remove(layer);

                var slot = ent.Comp.UnfoldedSlots ?? ent.Comp.FoldedSlots ?? SlotFlags.NONE;

                foreach (var layer in ent.Comp.UnfoldedHideLayers)
                    hideLayerComp.Layers[layer] = slot;

                Dirty(ent.Owner, hideLayerComp);
            }
            // Goobstation end
        }
    }
}
