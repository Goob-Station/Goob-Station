// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Common.CCVar;
using Content.Shared._Oskarrr.CustomGhost;
using Content.Shared.Ghost;
using Robust.Client.GameObjects;
using Robust.Shared.Configuration;

namespace Content.Client._Oskarrr.CustomGhost;

public sealed class CustomGhostVisualizer : VisualizerSystem<GhostComponent>
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    public override void Initialize()
    {
        base.Initialize();
        Subs.CVar(_configuration, GoobCVars.CustomGhostMaxSize, OnMaxSizeChanged);
    }

    private void OnMaxSizeChanged(int maxSquare)
    {
        var query = AllEntityQuery<GhostComponent, AppearanceComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out _, out var appearance, out var sprite))
        {
            ApplyScale(uid, appearance, sprite, maxSquare);
        }
    }

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
                }
                catch
                {
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
                }
            }
            else
            {
                args.Sprite.LayerSetRSI(0, spriteData);
            }

            ApplyScale(uid, args.Component, args.Sprite, _configuration.GetCVar(GoobCVars.CustomGhostMaxSize));
            return;
        }

        if (AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.AlphaOverride, out var alpha, args.Component))
        {
            args.Sprite.Color = args.Sprite.Color.WithAlpha(alpha);
        }
    }

    private void ApplyScale(EntityUid uid, AppearanceComponent appearance, SpriteComponent sprite, int maxSquare)
    {
        if (!AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.MaxSize, out var maxSize, appearance))
            return;

        var scale = 1f;

        if (maxSquare > 0
            && maxSize > 0f
            && sprite.AllLayers.FirstOrDefault()?.Rsi is { } rsi)
        {
            var largestSide = Math.Max(rsi.Size.X, rsi.Size.Y);
            var limit = maxSquare * maxSize;

            if (largestSide > limit && largestSide > 0)
                scale = limit / largestSide;
        }

        sprite.LayerSetScale(0, new Vector2(scale, scale));
    }
}
