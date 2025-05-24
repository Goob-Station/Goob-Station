using Content.Goobstation.Shared.Xenobiology;
using Content.Goobstation.Shared.Xenobiology.Components;
using Robust.Client.GameObjects;

namespace Content.Goobstation.Client.Xenobiology;

/// <summary>
/// This handles visual changes in slimes between breeds.
/// </summary>
public sealed class XenoSlimeVisualizerSystem : VisualizerSystem<SlimeComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, SlimeComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<Color>(uid, XenoSlimeVisuals.Color, out var color, args.Component))
        {
            foreach (var layer in args.Sprite.AllLayers)
            {
                layer.Color = color.WithAlpha(layer.Color.A);
            }
        }
    }
}
