// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Footprints;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Footprints;

public sealed class FootprintSystem : EntitySystem
{

    [Dependency] private readonly SpriteSystem _sprite = default!;

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
            if (!_sprite.LayerExists((entity, sprite), i))
                _sprite.AddBlankLayer((entity,sprite), i);

            _sprite.LayerSetOffset((entity, sprite), i, footprint.Footprints[i].Offset);
            _sprite.LayerSetRotation((entity, sprite), i, footprint.Footprints[i].Rotation);
            _sprite.LayerSetColor((entity, sprite), i, footprint.Footprints[i].Color);
            _sprite.LayerSetSprite((entity, sprite), i, new SpriteSpecifier.Rsi(new("/Textures/_CorvaxNext/Effects/footprint.rsi"), footprint.Footprints[i].State));
        }
    }
}
