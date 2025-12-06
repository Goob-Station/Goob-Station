// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared._Goobstation.Heretic.Systems;
using Robust.Client.GameObjects;

namespace Content.Client._Shitcode.Heretic;

public sealed class EntropicPlumeSystem : SharedEntropicPlumeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EntropicPlumeAffectedComponent, AfterAutoHandleStateEvent>(OnState);
    }

    private void OnState(Entity<EntropicPlumeAffectedComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveSprite((uid, sprite));
        AddSprite((uid, comp, sprite));
    }

    private void OnShutdown(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentShutdown args)
    {
        var (uid, _) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        RemoveSprite((uid, sprite));
    }

    private void OnStartup(Entity<EntropicPlumeAffectedComponent> ent, ref ComponentStartup args)
    {
        var (uid, comp) = ent;

        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        AddSprite((uid, comp, sprite));
    }

    private void RemoveSprite(Entity<SpriteComponent> ent)
    {
        var (uid, sprite) = ent;

        if (!sprite.LayerMapTryGet(EntropicPlumeKey.Key, out var layer))
            return;

        sprite.RemoveLayer(layer);
    }

    private void AddSprite(Entity<EntropicPlumeAffectedComponent, SpriteComponent> ent)
    {
        var (uid, comp, sprite) = ent;

        if (comp.Sprite == null)
            return;

        if (sprite.LayerMapTryGet(EntropicPlumeKey.Key, out _))
            return;

        var layer = sprite.AddLayer(comp.Sprite);
        sprite.LayerMapSet(EntropicPlumeKey.Key, layer);
    }
}
