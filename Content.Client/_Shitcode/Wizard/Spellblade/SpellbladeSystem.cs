// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Spellblade;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Wizard.Spellblade;

public sealed class SpellbladeSystem : SharedSpellbladeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShieldedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ShieldedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<ShieldedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (!sprite.LayerMapTryGet(ShieldedKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void OnStartup(Entity<ShieldedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(ShieldedKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerMapSet(ShieldedKey.Key, layer);
    }
}