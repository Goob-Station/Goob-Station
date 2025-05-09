// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 BombasterDS2 <shvalovdenis.workmail@gmail.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Client.Clothing;
using Content.Goobstation.Client.Clothing.Components;
using Content.Goobstation.Shared.Clothing;
using Content.Shared.Clothing;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Clothing.EntitySystems;

public sealed class SealableClothingVisualizerSystem : VisualizerSystem<SealableClothingVisualsComponent>
{
    [Dependency] private readonly SharedItemSystem _itemSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SealableClothingVisualsComponent, GetEquipmentVisualsEvent>(OnGetEquipmentVisuals, after: new[] { typeof(ClientClothingSystem) });
    }

    protected override void OnAppearanceChange(EntityUid uid, SealableClothingVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!AppearanceSystem.TryGetData<bool>(uid, SealableClothingVisuals.Sealed, out var isSealed, args.Component))
            return;

        if (args.Sprite != null && component.SpriteLayer != null && args.Sprite.LayerMapTryGet(component.SpriteLayer, out var layer))
        {
            args.Sprite.LayerSetVisible(layer, isSealed);
        }

        _itemSystem.VisualsChanged(uid);
    }

    private void OnGetEquipmentVisuals(Entity<SealableClothingVisualsComponent> sealable, ref GetEquipmentVisualsEvent args)
    {
        var (uid, comp) = sealable;

        if (!TryComp(uid, out AppearanceComponent? appearance)
            || !AppearanceSystem.TryGetData<bool>(uid, SealableClothingVisuals.Sealed, out var isSealed, appearance)
            || !isSealed)
            return;

        if (!comp.VisualLayers.TryGetValue(args.Slot, out var layers))
            return;

        var i = 0;
        foreach (var layer in layers)
        {
            var key = layer.MapKeys?.FirstOrDefault();
            if (key == null)
            {
                key = i == 0 ? $"{args.Slot}-sealed" : $"{args.Slot}-sealed-{i}";
                i++;
            }

            args.Layers.Add((key, layer));
        }
    }
}