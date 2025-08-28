// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Blob;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Blob;

public sealed class BlobTileVisualizerSystem : VisualizerSystem<BlobVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, BlobVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!Resolve(uid, ref args.Sprite)
            || !AppearanceSystem.TryGetData<Color>(uid, BlobColorVisuals.Color, out var color, args.Component))
            return;

        var layer = args.Sprite[BlobVisualLayers.Blob];
        layer.Color = color;
    }
}

public enum BlobVisualLayers : byte
{
    Blob,
}
