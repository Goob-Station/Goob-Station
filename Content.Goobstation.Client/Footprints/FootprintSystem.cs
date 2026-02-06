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
        SubscribeLocalEvent<FootprintComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<FootprintComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateSprite(ent, ent);
    }

    private void OnComponentStartup(Entity<FootprintComponent> entity, ref ComponentStartup e)
    {
        UpdateSprite(entity, entity);
    }

    private void UpdateSprite(EntityUid entity, FootprintComponent footprint)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        for (var i = 0; i < footprint.Footprints.Count; i++)
        {
            if (!sprite.LayerExists(i, false))
                sprite.AddBlankLayer(i);

            var print = footprint.Footprints[i];

            sprite.LayerSetOffset(i, print.Offset);
            sprite.LayerSetRotation(i, print.Rotation);
            sprite.LayerSetColor(i, print.Color);
            sprite.LayerSetSprite(i, print.Sprite);
        }
    }
}
