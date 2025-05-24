using Content.Client.Atmos.EntitySystems;
using Content.Client.DamageState;
using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Xenobiology;

/// <summary>
/// This handles...
/// </summary>
public sealed class SlimeColorVisualizerSystem : VisualizerSystem<SlimeComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, SlimeComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<Color>(uid, SlimeColorVisuals.Color, out var color, args.Component))
        {
            foreach (var layer in args.Sprite.AllLayers)
            {
                layer.Color = color.WithAlpha(layer.Color.A);
            }
        }
    }
}
