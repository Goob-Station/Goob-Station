// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Administration.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.Administration.Systems;

public sealed class KillSignSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<KillSignComponent, ComponentStartup>(KillSignAdded);
        SubscribeLocalEvent<KillSignComponent, ComponentShutdown>(KillSignRemoved);
    }

    private void KillSignRemoved(EntityUid uid, KillSignComponent component, ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(KillSignKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void KillSignAdded(EntityUid uid, KillSignComponent component, ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(KillSignKey.Key, out var _))
            return;

        var adj = sprite.Bounds.Height / 2 + ((1.0f/32) * 6.0f);

        var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(new ResPath("Objects/Misc/killsign.rsi"), "sign"));
        sprite.LayerMapSet(KillSignKey.Key, layer);

        sprite.LayerSetOffset(layer, new Vector2(0.0f, adj));
        sprite.LayerSetShader(layer, "unshaded");
    }

    private enum KillSignKey
    {
        Key,
    }
}