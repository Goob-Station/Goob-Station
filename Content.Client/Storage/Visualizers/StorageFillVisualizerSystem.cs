// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Storage.Visualizers;

public sealed class StorageFillVisualizerSystem : VisualizerSystem<StorageFillVisualizerComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, StorageFillVisualizerComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!TryComp(uid, out SpriteComponent? sprite))
            return;

        if (!AppearanceSystem.TryGetData<int>(uid, StorageFillVisuals.FillLevel, out var level, args.Component))
            return;

        var state = $"{component.FillBaseName}-{level}";
        args.Sprite.LayerSetState(StorageFillLayers.Fill, state);
    }
}