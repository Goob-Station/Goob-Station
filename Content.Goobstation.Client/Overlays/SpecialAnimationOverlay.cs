// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Shared.SpecialAnimation;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Overlays;

public sealed class SpecialAnimationOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    public Queue<SpecialAnimationData> AnimationQueue = new();

    private SpecialAnimationData? _currentAnimation;

    private IRenderTexture? _target;

    private (Font Font, string Path, int Size)? _font;

    public SpecialAnimationOverlay()
    {
        IoCManager.InjectDependencies(this);

        var path = SpecialAnimationData.DefaultAnimation.TextFontPath;
        var size = SpecialAnimationData.DefaultAnimation.TextFontSize;

        _font = (new VectorFont(_cache.GetResource<FontResource>(path), size), path, size);
        _target = CreateRenderTarget((64, 64), nameof(SpecialAnimationOverlay));
        ZIndex = 102;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity == null)
            return;

        // Set current animation if we don't have any
        if (_currentAnimation is null)
        {
            if (!AnimationQueue.TryDequeue(out _currentAnimation))
                return; // Nothing2Do

            if (!StartupAnimation(_currentAnimation))
                return; // Failed to make a sprite, it's over...
        }

        var curTime = _timing.CurTime;

        DebugTools.Assert(_currentAnimation.TotalDuration > _currentAnimation.FadeInDuration + _currentAnimation.FadeOutDuration);

        var endTime = _currentAnimation.StartTime + TimeSpan.FromSeconds(_currentAnimation.TotalDuration);

        // The animation is over, kill it
        if (endTime < curTime)
        {
            KillAnimation(_currentAnimation);
            _currentAnimation = null;
            return;
        }

        if (_currentAnimation.AnimationEntity == null)
            return;

        var anime = _currentAnimation.AnimationEntity.Value; // im going insane

        CalculateAnimation(_currentAnimation);

        // Draw everything on a screen
        if (!_entity.TryGetComponent(anime, out SpriteComponent? sprite))
            return;

        var screen = args.ScreenHandle;
        var uiScale = (args.ViewportControl as Control)?.UIScale ?? 1f;
        var center = _clyde.ScreenSize / 2;

        // Render sprite
        var targetSize = args.Viewport.RenderTarget.Size;
        if (_target?.Size != targetSize)
        {
            _target = _clyde
                .CreateRenderTarget(targetSize,
                    new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8Srgb),
                    name: nameof(SpecialAnimationOverlay));
        }

        screen.RenderInRenderTarget(_target,
            () =>
        {
            screen.DrawEntity(
                anime,
                center + _currentAnimation.Position,
                Vector2.One * uiScale * _currentAnimation.Scale,
                Angle.Zero,
                Angle.Zero,
                Direction.South,
                sprite);
        },
            Color.Transparent);

        var opacity = _currentAnimation.Opacity;
        screen.DrawTexture(_target.Texture, Vector2.Zero, Color.White.WithAlpha(opacity));

        // Render text
        if (_currentAnimation.Text == null)
            return;

        // Change our font if required
        if (_font == null ||
            _font.Value.Path != _currentAnimation.TextFontPath ||
            _font.Value.Size != _currentAnimation.TextFontSize)
        {
            var path = _currentAnimation.TextFontPath;
            var size = _currentAnimation.TextFontSize;
            _font = (new VectorFont(_cache.GetResource<FontResource>(path), size), path, size);
        }

        screen.DrawString(
            _font.Value.Font,
            center + _currentAnimation.TextPosition,
            _currentAnimation.Text,
            _currentAnimation.TextOverrideColor.WithAlpha(opacity));
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

    private bool StartupAnimation(SpecialAnimationData animation)
    {
        var source = _entity.GetEntity(animation.Source);

        if (!_entity.TryGetComponent<SpriteComponent>(source, out var sourceSprite))
            return false;

        // Copy the sprite component from source to the dummy entity.
        var dummyEnt = _entity.Spawn();
        var dummySprite = _entity.EnsureComponent<SpriteComponent>(dummyEnt);
        dummySprite.CopyFrom(sourceSprite);

        // Set some values
        animation.AnimationEntity = dummyEnt;
        animation.Position = animation.StartPosition;
        animation.StartTime = _timing.CurTime;
        animation.LastTime = _timing.CurTime;
        animation.Opacity = 0f;
        return true;
    }

    private void CalculateAnimation(SpecialAnimationData animation)
    {
        var curTime = _timing.CurTime;
        var frameTime = (float) (curTime - animation.LastTime).TotalSeconds;
        var fadeInEndTime = animation.StartTime + TimeSpan.FromSeconds(animation.FadeInDuration);
        var fadeOutStartTime = animation.StartTime + TimeSpan.FromSeconds(animation.TotalDuration) - TimeSpan.FromSeconds(animation.FadeOutDuration);

        // Move the sprite
        var distanceVec = animation.EndPosition - animation.StartPosition;
        var moveAmount = distanceVec * frameTime / animation.TotalDuration;
        animation.Position += moveAmount;

        // Fade-in
        if (fadeInEndTime > curTime)
        {
            var fadeInOpacityChange = animation.MaxOpacity / animation.FadeInDuration;
            animation.Opacity += fadeInOpacityChange * frameTime;
            animation.Opacity = MathF.Min(animation.Opacity, animation.MaxOpacity);
        }

        // Fade-out
        if (fadeOutStartTime < curTime)
        {
            var fadeOutOpacityChange = animation.MaxOpacity / animation.FadeOutDuration;
            animation.Opacity -= fadeOutOpacityChange * frameTime;
            animation.Opacity = MathF.Max(animation.Opacity, 0f);
        }

        animation.LastTime = curTime;
    }

    private void KillAnimation(SpecialAnimationData animation)
    {
        _entity.QueueDeleteEntity(animation.AnimationEntity);
    }
}
