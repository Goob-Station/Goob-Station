// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
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

using Content.Shared._Shitcode.Heretic.Systems;
using Content.Shared.Heretic;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client._Shitcode.Heretic;

public sealed class HereticCombatMarkSystem : SharedHereticCombatMarkSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HereticCombatMarkComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<HereticCombatMarkComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<HereticCombatMarkComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        AddLayer(ent);
    }

    private void OnStartup(Entity<HereticCombatMarkComponent> ent, ref ComponentStartup args)
    {
        AddLayer(ent);
    }

    private void AddLayer(Entity<HereticCombatMarkComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        var state = ent.Comp.Path.ToLower();

        if (sprite.LayerMapTryGet(HereticCombatMarkKey.Key, out var layer))
        {
            sprite.LayerSetState(layer, state);
            return;
        }

        var rsi = new SpriteSpecifier.Rsi(ent.Comp.ResPath, state);

        layer = sprite.AddLayer(rsi);
        sprite.LayerMapSet(HereticCombatMarkKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }

    private void OnShutdown(Entity<HereticCombatMarkComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(HereticCombatMarkKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }
}
