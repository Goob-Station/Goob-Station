// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Changeling;

public sealed class FleshmendEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FleshmendEffectComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FleshmendEffectComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FleshmendEffectComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<FleshmendEffectComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        AddLayer(ent);
    }

    private void OnStartup(Entity<FleshmendEffectComponent> ent, ref ComponentStartup args)
    {
        AddLayer(ent);
    }

    private void OnShutdown(Entity<FleshmendEffectComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(FleshmendEffectKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void AddLayer(Entity<FleshmendEffectComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var state = ent.Comp.EffectState;

        if (sprite.LayerMapTryGet(FleshmendEffectKey.Key, out var layer))
        {
            sprite.LayerSetState(layer, state);
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(ent.Comp.ResPath, state);

        layer = sprite.AddLayer(rsi);
        sprite.LayerMapSet(FleshmendEffectKey.Key, layer);
    }
}
