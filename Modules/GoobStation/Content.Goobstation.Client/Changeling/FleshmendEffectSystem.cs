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
    [Dependency] private readonly SpriteSystem _sprite = default!;

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
        if (TryComp<FleshmendComponent>(ent, out var fleshmend) // only done if new effects were yaml'd in (or just applied to the comp)
            && fleshmend.EffectState != null
            && fleshmend.ResPath != ResPath.Empty)
        {
            ent.Comp.EffectState = fleshmend.EffectState;
            ent.Comp.ResPath = fleshmend.ResPath;
        }

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
