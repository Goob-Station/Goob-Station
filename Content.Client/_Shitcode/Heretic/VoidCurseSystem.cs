// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 whateverusername0 <whateveremail>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic;

public sealed partial class VoidCurseSystem : SharedVoidCurseSystem
{
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
            if (!TryComp<SpriteComponent>(uid, out var sprite))
                continue;

            if (!sprite.LayerMapTryGet(0, out var layer))
                continue;

            var state = _overlayStateNormal;
            if (comp.Stacks >= comp.MaxStacks / 1.5f)
                state = _overlayStateMax;

            sprite.LayerSetState(layer, state);
        }
    }

    private void OnStartup(Entity<VoidCurseComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (sprite.LayerMapTryGet(0, out var l))
        {
            sprite.LayerSetState(l, _overlayStateNormal);
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Heretic/void_overlay.rsi"), _overlayStateNormal);
        var layer = sprite.AddLayer(rsi);

        sprite.LayerMapSet(0, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }
    private void OnShutdown(Entity<VoidCurseComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(0, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }
}