using Robust.Client.GameObjects;
using Robust.Shared.Timing;

// ReSharper disable once CheckNamespace
namespace Content.Client.IconSmoothing;

public sealed partial class IconSmoothSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private Dictionary<string, double> _syncRsis = [];

    private void FixSpriteAnimations(SpriteComponent sprite)
    {
        if (sprite.BaseRSI == null)
            return;

        sprite.LayerSetAnimationTime(CornerLayers.NE, 0);
        sprite.LayerSetAnimationTime(CornerLayers.SE, 0);
        sprite.LayerSetAnimationTime(CornerLayers.NW, 0);
        sprite.LayerSetAnimationTime(CornerLayers.SW, 0);

        var rsi = sprite.BaseRSI.Path.CanonPath;
        var curTime = _timing.CurTime.TotalSeconds;

        if (!_syncRsis.TryGetValue(rsi, out var rsiTime))
        {
            _syncRsis.Add(sprite.BaseRSI.Path.CanonPath, curTime);
            return;
        }

        var diffTime = (float) (curTime - rsiTime);

        sprite.LayerSetAnimationTime(CornerLayers.NE, diffTime);
        sprite.LayerSetAnimationTime(CornerLayers.SE, diffTime);
        sprite.LayerSetAnimationTime(CornerLayers.NW, diffTime);
        sprite.LayerSetAnimationTime(CornerLayers.SW, diffTime);
    }
}
