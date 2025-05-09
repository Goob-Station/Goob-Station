using System.Numerics;
using Content.Goobstation.Shared.SpellCard;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Graphics;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Overlays;

public sealed class SpellCardOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    public Queue<SpellCardAnimationData> AnimationQueue = new();

    private SpellCardAnimationData? _currentAnimation;

    private IRenderTexture? _target;

    private Font _font;

    public SpellCardOverlay()
    {
        IoCManager.InjectDependencies(this);

        _font = new VectorFont( _cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 20);
        _target = CreateRenderTarget((64, 64), nameof(SpellCardOverlay));

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
                    name: nameof(SpellCardOverlay));
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
        screen.DrawString(_font, center + _currentAnimation.TextPosition, _currentAnimation.Name ?? "Unknown", Color.White.WithAlpha(opacity));
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

    private bool StartupAnimation(SpellCardAnimationData animation)
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

    private void CalculateAnimation(SpellCardAnimationData animation)
    {
        var curTime = _timing.CurTime;
        var frameTime = (float) (curTime - animation.LastTime).TotalSeconds;
        var fadeInEndTime = animation.StartTime + TimeSpan.FromSeconds(animation.FadeInDuration);
        var fadeOutStartTime = animation.StartTime + TimeSpan.FromSeconds(animation.TotalDuration) - TimeSpan.FromSeconds(animation.FadeOutDuration);

        // Handle animating all fades, movement, and opacity of the current animation.
        var moveAmount = animation.MoveAngle.ToVec() * animation.MoveSpeed;
        animation.Position += moveAmount * frameTime;

        // Fade-in
        if (fadeInEndTime > curTime)
        {
            animation.Opacity += animation.FadeInOpacityChange * frameTime;
        }

        // Fade-out
        if (fadeOutStartTime < curTime)
        {
            animation.Opacity -= animation.FadeOutOpacityChange * frameTime;
        }

        animation.LastTime = curTime;
    }

    private void KillAnimation(SpellCardAnimationData animation)
    {
        _entity.QueueDeleteEntity(animation.AnimationEntity);
    }
}
