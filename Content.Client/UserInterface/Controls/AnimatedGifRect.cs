using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Timing;

namespace Content.Client.UserInterface.Controls;

/// <summary>
///     A <see cref="TextureRect"/> that can additionally play back a decoded gif as a sequence of frame
///     textures with per-frame delays. Used for animated lobby backgrounds.
/// </summary>
/// <remarks>
///     The control owns the frame textures it is given and disposes them whenever they are replaced
///     (<see cref="SetGif"/>/<see cref="SetStatic"/>) — nothing else frees them, since UI controls are
///     removed rather than disposed. Always go through those two methods instead of assigning
///     <see cref="TextureRect.Texture"/> directly.
/// </remarks>
public sealed class AnimatedGifRect : TextureRect
{
    private Texture[]? _frames;
    private float[]? _delays;
    private int _currentFrame;
    private float _frameTime;

    /// <summary>
    ///     Plays the given gif frames on a loop. Gifs are letterboxed (<see cref="StretchMode.KeepAspectCentered"/>)
    ///     so arbitrary aspect ratios are shown in full rather than cropped.
    /// </summary>
    public void SetGif(Texture[] frames, float[] delays)
    {
        DisposeFrames();

        _frames = frames;
        _delays = delays;
        _currentFrame = 0;
        _frameTime = 0f;
        Stretch = StretchMode.KeepAspectCentered;
        Texture = frames.Length > 0 ? frames[0] : null;
    }

    /// <summary>
    ///     Displays a single static texture, stopping any running animation. Keeps the classic
    ///     full-bleed <see cref="StretchMode.KeepAspectCovered"/> look of static lobby backgrounds.
    /// </summary>
    public void SetStatic(Texture? texture)
    {
        DisposeFrames();

        Stretch = StretchMode.KeepAspectCovered;
        Texture = texture;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!VisibleInTree || _frames is not { Length: > 1 } frames || _delays is not { } delays)
            return;

        _frameTime += args.DeltaSeconds;

        // Advance as many frames as the elapsed time covers (handles long pauses / very short delays).
        // The delay floor guards against a zero delay spinning this loop forever.
        var advanced = false;
        while (_frameTime >= MathF.Max(delays[_currentFrame], 0.001f))
        {
            _frameTime -= MathF.Max(delays[_currentFrame], 0.001f);
            _currentFrame = (_currentFrame + 1) % frames.Length;
            advanced = true;
        }

        if (advanced)
            Texture = frames[_currentFrame];
    }

    private void DisposeFrames()
    {
        if (_frames != null)
        {
            foreach (var frame in _frames)
                (frame as IDisposable)?.Dispose();
        }

        _frames = null;
        _delays = null;
        Texture = null;
    }
}
