// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.DamageState;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Xenobiology;

/// <summary>
/// This handles visual changes in mobs which can transition growth states.
/// </summary>
public sealed class MobGrowthVisualizerSystem : VisualizerSystem<MobGrowthComponent>
{

    [Dependency] private readonly SpriteSystem _sprite = default!;

    //I have a feeling this may need some protective functions.
    protected override void OnAppearanceChange(EntityUid uid, MobGrowthComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null
            || !AppearanceSystem.TryGetData<string>(uid, GrowthStateVisuals.Sprite, out var rsi, args.Component))
            return;
        _sprite.LayerSetRsi((uid, args.Sprite), DamageStateVisualLayers.Base, new ResPath(rsi));
    }
}
