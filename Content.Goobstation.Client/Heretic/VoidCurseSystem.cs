// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Heretic;

public sealed class VoidCurseSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidCurseComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidCurseComponent, ComponentShutdown>(OnShutdown);
    }

    private readonly string _overlayStateNormal = "void_chill_partial",
                            _overlayStateMax = "void_chill_oh_fuck";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<VoidCurseComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (!TryComp<SpriteComponent>(uid, out var sprite)
            || !_sprite.LayerMapTryGet((uid, sprite), VoidCurseLayer.Base, out var layer, false))
                continue;

            var state = _overlayStateNormal;
            if (comp.Stacks >= comp.MaxStacks)
                state = _overlayStateMax;

            _sprite.LayerSetRsiState((uid, sprite), layer, state);
        }
    }

    private void OnStartup(Entity<VoidCurseComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (_sprite.LayerMapTryGet((ent, sprite), VoidCurseLayer.Base, out var l, false))
        {
            _sprite.LayerSetRsiState((ent, sprite), l, _overlayStateNormal);
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/void_overlay.rsi"), _overlayStateNormal);
        var layer = _sprite.AddLayer((ent, sprite),rsi);

        _sprite.LayerMapSet((ent, sprite), VoidCurseLayer.Base, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }
    private void OnShutdown(Entity<VoidCurseComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!_sprite.LayerMapTryGet((ent, sprite), VoidCurseLayer.Base, out var layer, false))
            return;

        _sprite.RemoveLayer((ent, sprite), layer);
    }
}

public enum VoidCurseLayer
{
    Base
}
