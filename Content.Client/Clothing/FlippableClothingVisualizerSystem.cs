// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nikolai Korolev <korolevns98@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Foldable;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client.Clothing;

public sealed class FlippableClothingVisualizerSystem : VisualizerSystem<FlippableClothingVisualsComponent>
{
    [Dependency] private readonly SharedItemSystem _itemSys = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlippableClothingVisualsComponent, GetEquipmentVisualsEvent>(OnGetVisuals, after: [typeof(ClothingSystem)]);
        SubscribeLocalEvent<FlippableClothingVisualsComponent, FoldedEvent>(OnFolded);
    }

    private void OnFolded(Entity<FlippableClothingVisualsComponent> ent, ref FoldedEvent args)
    {
        _itemSys.VisualsChanged(ent);
    }

    private void OnGetVisuals(Entity<FlippableClothingVisualsComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite) ||
            !TryComp(ent, out ClothingComponent? clothing))
            return;

        if (clothing.MappedLayer == null ||
            !AppearanceSystem.TryGetData<bool>(ent, FoldableSystem.FoldedVisuals.State, out var folding) ||
            !sprite.LayerMapTryGet(folding ? ent.Comp.FoldingLayer : ent.Comp.UnfoldingLayer, out var idx))
            return;

        // add each layer to the visuals
        var spriteLayer = sprite[idx];
        foreach (var layer in args.Layers)
        {
            if (layer.Item1 != clothing.MappedLayer)
                continue;

            layer.Item2.Scale = spriteLayer.Scale;
        }
    }
}