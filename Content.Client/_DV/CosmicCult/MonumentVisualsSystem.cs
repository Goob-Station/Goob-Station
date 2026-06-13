// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.GameObjects;
using Content.Shared._DV.CosmicCult.Components;

namespace Content.Client._DV.CosmicCult;

/// <summary>
/// Visualizer for The Monument of the Cosmic Cult.
/// </summary>
public sealed class MonumentVisualizerSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MonumentComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<MonumentComponent> ent, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;
        _sprite.LayerMapTryGet((ent, args.Sprite), MonumentVisualLayers.TransformLayer, out var transformLayer, false);
        _sprite.LayerMapTryGet((ent, args.Sprite), MonumentVisualLayers.MonumentLayer, out var baseLayer, false);
        _appearance.TryGetData<bool>(ent, MonumentVisuals.Transforming, out var transforming, args.Component);
        _appearance.TryGetData<bool>(ent, MonumentVisuals.Tier3, out var tier3, args.Component);

        if (!tier3)
            _sprite.LayerSetRsiState((ent, args.Sprite), transformLayer, "transform-stage2");
        else
            _sprite.LayerSetRsiState((ent, args.Sprite), transformLayer, "transform-stage3");
        if (transforming && HasComp<MonumentTransformingComponent>(ent))
        {
            _sprite.LayerSetAnimationTime((ent, args.Sprite), transformLayer, 0f);
            _sprite.LayerSetVisible((ent, args.Sprite), transformLayer, true);
            _sprite.LayerSetVisible((ent, args.Sprite), baseLayer, false);
        }
        else
        {
            _sprite.LayerSetVisible((ent, args.Sprite), transformLayer, false);
            _sprite.LayerSetVisible((ent, args.Sprite), baseLayer, true);
        }
    }
}
