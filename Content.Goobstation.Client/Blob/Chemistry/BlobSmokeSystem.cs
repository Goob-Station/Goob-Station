// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Blob.Chemistry;

public sealed class BlobSmokeSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<Shared.Blob.Chemistry.BlobSmokeColorComponent, AfterAutoHandleStateEvent>(OnBlobTileHandleState);
    }

    private void OnBlobTileHandleState(EntityUid uid, Shared.Blob.Chemistry.BlobSmokeColorComponent component, ref AfterAutoHandleStateEvent state)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        for (var i = 0; i < sprite.AllLayers.Count(); i++)
        {
            sprite.LayerSetColor(i, component.Color);
        }
    }
}