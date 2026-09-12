// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Spellblade.Components;
using Content.Goobstation.Shared.Spellblade.Systems;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Spellblade.Systems;

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