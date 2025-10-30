// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.SpecialPassives.Fleshmend.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.SpecialPassives.Fleshmend;

public sealed class FleshmendEffectSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FleshmendEffectComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FleshmendEffectComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<FleshmendEffectComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.ResPath == ResPath.Empty
            || ent.Comp.EffectState == null)
            return;

        AddLayer(ent);
    }

    private void OnShutdown(Entity<FleshmendEffectComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent, sprite), FleshmendEffectKey.Key, out var layer, false))
            return;

        _sprite.RemoveLayer((ent, sprite), layer);
    }

    private void AddLayer(Entity<FleshmendEffectComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var state = ent.Comp.EffectState;

        if (_sprite.LayerMapTryGet((ent, sprite), FleshmendEffectKey.Key, out var layer, false))
        {
            _sprite.LayerSetRsiState((ent, sprite), layer, state);
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(ent.Comp.ResPath, state);

        layer = _sprite.AddLayer((ent, sprite), rsi);
        _sprite.LayerMapSet((ent, sprite), FleshmendEffectKey.Key, layer);
    }
}
