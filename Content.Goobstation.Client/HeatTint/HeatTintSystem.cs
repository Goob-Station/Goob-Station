using Content.Goobstation.Shared.HeatTint;
using Robust.Client.GameObjects;
using Robust.Shared.Reflection;

namespace Content.Goobstation.Client.HeatTint;

public sealed class HeatTintSystem : SharedHeatTintSystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly IReflectionManager _reflection = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeatTintComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private static Color GetBaseColor(HeatTintComponent comp, SpriteComponent sprite, int layerIndex)
    {
        var currentColor = sprite[layerIndex].Color;

        if (comp.LastAppliedColors.TryGetValue(layerIndex, out var lastApplied) && currentColor != lastApplied)
            comp.BaseColors[layerIndex] = currentColor;

        if (!comp.BaseColors.TryGetValue(layerIndex, out var baseColor))
        {
            baseColor = currentColor;
            comp.BaseColors[layerIndex] = baseColor;
        }

        return baseColor;
    }

    private void ApplyHeatColor(HeatTintComponent comp, SpriteComponent sprite, EntityUid uid, int index, Color heatColor)
    {
        var final = GetBaseColor(comp, sprite, index) * heatColor;
        _sprite.LayerSetColor((uid, sprite), index, final);
        comp.LastAppliedColors[index] = final;
    }

    private void OnAppearanceChange(EntityUid uid, HeatTintComponent comp, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (!_appearance.TryGetData<float>(uid, HeatTintVisuals.Temperature, out var temperature, args.Component))
            return;

        var heatColor = GetHeatColor(comp.ColorGradient, temperature);

        if (comp.AffectedLayers != null && comp.AffectedLayers.Count > 0)
        {
            foreach (var layerKey in comp.AffectedLayers)
            {
                if (_reflection.TryParseEnumReference(layerKey, out var @enum))
                {
                    if (!_sprite.LayerMapTryGet((uid, args.Sprite), @enum, out var index, false))
                        continue;

                    ApplyHeatColor(comp, args.Sprite, uid, index, heatColor);
                }
                else
                {
                    if (!_sprite.LayerMapTryGet((uid, args.Sprite), layerKey, out var index, false))
                        continue;

                    ApplyHeatColor(comp, args.Sprite, uid, index, heatColor);
                }
            }
        }
        else
        {
            _sprite.SetColor((uid, args.Sprite), heatColor);
        }
    }
}
