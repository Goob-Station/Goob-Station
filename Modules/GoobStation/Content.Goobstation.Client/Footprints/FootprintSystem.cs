// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Footprints;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Footprints;

public sealed class FootprintSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<FootprintComponent, ComponentStartup>(OnComponentStartup);
        SubscribeNetworkEvent<FootprintChangedEvent>(OnFootprintChanged);
    }

    private void OnComponentStartup(Entity<FootprintComponent> entity, ref ComponentStartup e)
    {
        UpdateSprite(entity, entity);
    }

    private void OnFootprintChanged(FootprintChangedEvent e)
    {
        if (!TryGetEntity(e.Entity, out var entity))
            return;

        if (!TryComp<FootprintComponent>(entity, out var footprint))
            return;

        UpdateSprite(entity.Value, footprint);
    }

    private void UpdateSprite(EntityUid entity, FootprintComponent footprint)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        for (var i = 0; i < footprint.Footprints.Count; i++)
        {
            if (!sprite.LayerExists(i, false))
                sprite.AddBlankLayer(i);

            sprite.LayerSetOffset(i, footprint.Footprints[i].Offset);
            sprite.LayerSetRotation(i, footprint.Footprints[i].Rotation);
            sprite.LayerSetColor(i, footprint.Footprints[i].Color);
            sprite.LayerSetSprite(i, new SpriteSpecifier.Rsi(new("/Textures/_CorvaxNext/Effects/footprint.rsi"), footprint.Footprints[i].State));
        }
    }
}
