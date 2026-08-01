// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Explosion.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Explosion;

public sealed class ClusterGrenadeVisualizerSystem : VisualizerSystem<ClusterGrenadeVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, ClusterGrenadeVisualsComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<int>(uid, ClusterGrenadeVisuals.GrenadesCounter, out var grenadesCounter, args.Component))
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, $"{comp.State}-{grenadesCounter}");
    }
}