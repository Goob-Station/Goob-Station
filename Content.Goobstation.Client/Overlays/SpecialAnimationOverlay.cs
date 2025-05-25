// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Client.OverlaysAnimation;
using Content.Goobstation.Shared.OverlaysAnimation.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics;

namespace Content.Goobstation.Client.Overlays;

public sealed class SpecialAnimationOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    private readonly OverlayAnimationSystem _system = default!;

    private IRenderTexture? _target;

    private readonly Dictionary<(string Path, int Size), Font> _fontDict = new();

    public SpecialAnimationOverlay()
    {
        IoCManager.InjectDependencies(this);

        _system = _entity.EntitySysManager.GetEntitySystem<OverlayAnimationSystem>();

        _target = CreateRenderTarget((64, 64), nameof(SpecialAnimationOverlay));
        ZIndex = 200;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_system.AnimationsEnabled)
            return;

        // That is optimized I swear!
        DrawSprites(args);
        DrawTexts(args);
        DrawRectangles(args);
        DrawCircles(args);
    }

    private void DrawSprites(OverlayDrawArgs args)
    {
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = _clyde.ScreenSize / 2;

        var spriteQuery = _entity.EntityQueryEnumerator<OverlayAnimationComponent, OverlaySpriteComponent, SpriteComponent>();
        while (spriteQuery.MoveNext(out var uid, out var animationComp, out _, out var spriteComp))
        {
            var screen = args.ScreenHandle;

            // Render sprite
            var targetSize = args.Viewport.RenderTarget.Size;
            if (_target?.Size != targetSize)
            {
                _target = _clyde
                    .CreateRenderTarget(targetSize,
                        new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8),
                        name: nameof(SpecialAnimationOverlay));
            }

            screen.RenderInRenderTarget(_target,
                () =>
                {
                    screen.DrawEntity(
                        uid,
                        center + animationComp.Position * uiScale,
                        Vector2.One * uiScale * animationComp.Scale,
                        Angle.Zero,
                        Angle.Zero,
                        Direction.South,
                        spriteComp);
                },
                Color.Transparent);

            var color = animationComp.Color.WithAlpha(animationComp.Opacity);
            screen.DrawTexture(_target.Texture, Vector2.Zero, color);
        }
    }

    private void DrawTexts(OverlayDrawArgs args)
    {
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = _clyde.ScreenSize / 2;

        var textQuery = _entity.EntityQueryEnumerator<OverlayAnimationComponent, OverlayTextComponent>();
        while (textQuery.MoveNext(out var animationComp, out var textComp))
        {
            var screen = args.ScreenHandle;

            var fontPath = textComp.TextFontPath;
            var fontSize = textComp.TextFontSize;
            if (!_fontDict.TryGetValue( (fontPath, fontSize), out var font))
            {
                // Resolve the font if it wasn't loaded
                font = new VectorFont(_cache.GetResource<FontResource>(textComp.TextFontPath), textComp.TextFontSize);
                _fontDict.TryAdd( (fontPath, fontSize), font);
            }

            var color = animationComp.Color.WithAlpha(animationComp.Opacity);
            screen.DrawString(font, animationComp.Position + center * uiScale, textComp.Text, color);
        }
    }

    private void DrawRectangles(OverlayDrawArgs args)
    {
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = _clyde.ScreenSize / 2;

        var textQuery = _entity.EntityQueryEnumerator<OverlayAnimationComponent, OverlayRectComponent>();
        while (textQuery.MoveNext(out var animationComp, out var rectComp))
        {
            var screen = args.ScreenHandle;
            var color = animationComp.Color.WithAlpha(animationComp.Opacity);
            var position = (animationComp.Position + center) * uiScale;
            var box = new UIBox2(position, rectComp.BoxSize * uiScale);

            screen.DrawRect(box, color);
        }
    }

    private void DrawCircles(OverlayDrawArgs args)
    {
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = _clyde.ScreenSize / 2;

        var textQuery = _entity.EntityQueryEnumerator<OverlayAnimationComponent, OverlayCircleComponent>();
        while (textQuery.MoveNext(out var animationComp, out var circleComp))
        {
            var screen = args.ScreenHandle;
            var color = animationComp.Color.WithAlpha(animationComp.Opacity);
            var position = (animationComp.Position + center) * uiScale;

            screen.DrawCircle(position, circleComp.Radius * uiScale, color);
        }
    }

    private IRenderTexture CreateRenderTarget(Vector2i size, string name)
    {
        return _clyde.CreateRenderTarget(
            size,
            new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb, true),
            new TextureSampleParameters
            {
                Filter = true
            },
            name);
    }

    protected override void DisposeBehavior()
    {
        base.DisposeBehavior();
        _target?.Dispose();
    }
}
