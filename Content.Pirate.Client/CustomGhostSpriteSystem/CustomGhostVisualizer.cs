using Content.Client.Ghost;
using Content.Shared.Ghost;
using Content.Pirate.Shared.CustomGhostSystem;
using Robust.Client.GameObjects;

namespace Content.Pirate.Client.CustomGhostSpriteSystem;

public sealed class CustomGhostVisualizer : VisualizerSystem<GhostComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, GhostComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, CustomGhostAppearance.Sprite, out var spriteData, args.Component))
        {
            var split = spriteData.Split(':');
            if (split.Length == 2)
            {
                var rsiPath = split[0];
                var state = split[1];
                args.Sprite.LayerSetRSI(0, rsiPath);
                try
                {
                    args.Sprite.LayerSetState(0, state);
                    return;
                }
                catch
                {
                    // Якщо state не існує, підміняємо на перший доступний у RSI
                    var rsi = args.Sprite[0].Rsi;
                    if (rsi != null)
                    {
                        string[] fallbackStates = { "icon", "default", "static", "animated" };
                        foreach (var fallbackState in fallbackStates)
                        {
                            if (rsi.TryGetState(fallbackState, out _))
                            {
                                args.Sprite.LayerSetState(0, fallbackState);
                                break;
                            }
                        }
                    }
                    return;
                }
            }
            else
            {
                args.Sprite.LayerSetRSI(0, spriteData);
                return;
            }
        }

        if (AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.AlphaOverride, out var alpha, args.Component))
        {
            args.Sprite.Color = args.Sprite.Color.WithAlpha(alpha);
        }
    }
}
