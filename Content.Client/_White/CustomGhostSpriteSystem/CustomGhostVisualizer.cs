using System.Numerics;
using Content.Shared.Ghost;
using Content.Shared._White.CustomGhostSystem;
using Robust.Client.GameObjects;

namespace Content.Client._White.CustomGhostSpriteSystem;

public sealed class CustomGhostVisualizer : VisualizerSystem<GhostComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, GhostComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<string>(uid, CustomGhostAppearance.Sprite, out var rsiPath, args.Component))
        {
            args.Sprite.LayerSetRSI(0, rsiPath);
        }

        if (AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.AlphaOverride, out var alpha, args.Component))
        {
            args.Sprite.Color = args.Sprite.Color.WithAlpha(alpha);
        }

        if (AppearanceSystem.TryGetData<Vector2>(uid, CustomGhostAppearance.SizeOverride, out var size, args.Component))
        {
            args.Sprite.Scale = size;
        }
    }
} 
