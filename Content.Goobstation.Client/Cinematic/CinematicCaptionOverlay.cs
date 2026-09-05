using System.Numerics;
using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Displays text that gets written in on the users screen.
/// </summary>
public sealed class CinematicCaptionOverlay : Overlay
{
    /// <summary>
    /// Font sizes are authored against this viewport height and scaled from there.
    /// </summary>
    private const float ReferenceViewportHeight = 1080f;

    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private CaptionTargets? _targets;

    public ShaderInstance? AuraShader;
    public ShaderInstance? BlurShader;

    private readonly CaptionLayout _layout = new();

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public CinematicCaptionOverlay()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = 205;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_playerManager.LocalEntity is not { Valid: true } player
            || !_entityManager.HasComponent<CinematicCaptionComponent>(player))
            return false;

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_playerManager.LocalEntity is not { Valid: true } player)
            return;

        if (!_entityManager.TryGetComponent<CinematicCaptionComponent>(player, out var caption)
            || caption.Target.Length == 0)
            return;

        var bounds = args.ViewportBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var handle = args.ScreenHandle;
        if (!TryLayout(handle, caption, bounds, out var layout))
            return;

        var targets = EnsureTargets(bounds.Size, caption);
        var strength = Strength(caption, layout);

        RenderMask(handle, targets, caption, layout, bounds);

        if (!caption.AuraEnabled)
        {
            DrawPlain(handle, targets, caption, layout, bounds, strength);
            return;
        }

        if (AuraShader is not { } aura || BlurShader is not { } blur)
            return;

        BuildPyramid(handle, targets, caption, blur);
        Composite(handle, targets, caption, layout, bounds, strength, aura);
    }

    private float Strength(CinematicCaptionComponent caption, CaptionLayout layout)
    {
        var strength = caption.IgniteTime > 0f
            ? Math.Clamp(layout.Age / caption.IgniteTime, 0f, 1f)
            : 1f;

        if (_entityManager.TryGetComponent<CinematicComponent>(_playerManager.LocalEntity, out var cinematic))
            strength = MathF.Min(strength, cinematic.Strength);

        return strength;
    }

    private bool TryLayout(DrawingHandleScreen handle,
        CinematicCaptionComponent caption,
        UIBox2i bounds,
        out CaptionLayout layout)
    {
        layout = _layout;

        var viewportScale = bounds.Height / ReferenceViewportHeight;
        var font = RasterFont(caption, viewportScale);
        var age = Step(caption.Age, caption.StepRate);
        var size = caption.FontSize * viewportScale;
        var animated = size;
        if (caption.SlamScale > 0f)
            animated *= 1f + caption.SlamScale * MathF.Exp(-age * caption.SlamDecay);
        if (caption.Kick > 0f && age >= caption.KickTime)
            animated *= 1f + caption.Kick * MathF.Exp(-(age - caption.KickTime) * caption.KickDecay);
        if (caption.Throb > 0f)
            animated *= 1f + caption.Throb * MathF.Sin(age * 2.6f);

        layout.Font = font;
        layout.Age = age;
        layout.Time = Step((float) _timing.CurTime.TotalSeconds, caption.StepRate);
        layout.Scale = MathF.Max(animated, 1f) / font.Size;
        layout.GlyphSize = font.Size * layout.Scale;
        layout.Tracking = caption.Tracking * font.Size;
        layout.LineStep = font.Size * caption.LineSpacing;

        var restScale = MathF.Max(size, 1f) / font.Size;

        Wrap(handle, layout, caption.Target, bounds.Width * caption.MaxWidthFraction / restScale);
        if (layout.Lines.Count == 0)
            return false;

        var widest = 0f;
        var glyphs = 0;
        layout.Widths.Clear();

        foreach (var line in layout.Lines)
        {
            var width = Measure(handle, font, line, layout.Tracking);
            layout.Widths.Add(width);
            widest = MathF.Max(widest, width);
            glyphs += line.Length;
        }

        if (caption.Cursor.Length > 0)
            Measure(handle, font, caption.Cursor, layout.Tracking);

        layout.Shown = (int) MathF.Ceiling(Math.Clamp(caption.Progress, 0f, 1f) * glyphs);
        layout.SubjectFont = null;
        layout.SubjectTracking = 0f;
        layout.SubjectWidth = 0f;
        layout.SubjectTop = 0f;

        var subjectHeight = 0f;

        if (caption.Subject.Length > 0)
        {
            var subjectFont = RasterFont(caption, viewportScale, caption.SubjectScale);

            layout.SubjectFont = subjectFont;
            layout.SubjectTracking = caption.SubjectTracking * subjectFont.Size;
            layout.SubjectWidth = Measure(handle, subjectFont, caption.Subject, layout.SubjectTracking);
            layout.SubjectTop = -subjectFont.Size * caption.LineSpacing * (1f + caption.SubjectGap);

            subjectHeight = -layout.SubjectTop * layout.Scale;
        }

        widest = MathF.Max(widest, layout.SubjectWidth);

        var lineHeight = layout.LineStep * layout.Scale;
        var blockHeight = layout.Lines.Count * lineHeight;
        var centerX = bounds.Left + bounds.Width / 2f;
        var top = bounds.Top + bounds.Height * caption.VerticalPosition - blockHeight / 2f;
        var lowest = bounds.Bottom - blockHeight - lineHeight * 0.5f;
        var highest = bounds.Top + lineHeight * 0.5f + subjectHeight;
        if (lowest > highest)
            top = Math.Clamp(top, highest, lowest);

        layout.Anchor = new Vector2(centerX, top);

        var half = widest * layout.Scale / 2f;
        var pad = layout.GlyphSize * 2.6f * caption.AuraScale;

        layout.Block = new UIBox2(
            MathF.Max(centerX - half - pad, bounds.Left),
            MathF.Max(top - subjectHeight - pad, bounds.Top),
            MathF.Min(centerX + half + pad, bounds.Right),
            MathF.Min(top + blockHeight + pad, bounds.Bottom));

        return true;
    }

    private VectorFont RasterFont(CinematicCaptionComponent caption, float viewportScale, float sizeScale = 1f)
    {
        var resource = _cache.GetResource<FontResource>(caption.FontPath);
        var min = CinematicCaptionComponent.MinGlyphRasterSize;
        var max = CinematicCaptionComponent.MaxGlyphSheetExtent;
        var size = Math.Max(min, (int) (caption.FontSize * viewportScale * sizeScale));
        var font = new VectorFont(resource, size);
        var height = font.GetHeight(1f);
        if (height <= max)
            return font;

        size = Math.Max(min, size * max / height);
        return new VectorFont(resource, size);
    }

    private static float Step(float time, float rate)
        => rate > 0f ? MathF.Floor(time * rate) / rate : time;

    private static void Wrap(DrawingHandleScreen handle, CaptionLayout layout, string text, float maxWidth)
    {
        layout.Lines.Clear();
        var current = string.Empty;

        foreach (var word in text.Split(' '))
        {
            var candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && Measure(handle, layout.Font, candidate, layout.Tracking) > maxWidth)
            {
                layout.Lines.Add(current);
                current = word;
            }
            else
                current = candidate;
        }

        if (current.Length > 0)
            layout.Lines.Add(current);
    }

    private static float Measure(DrawingHandleScreen handle, VectorFont font, string line, float tracking)
        => handle.GetDimensions(font, line, 1f).X + tracking * Math.Max(0, line.Length - 1);

    private static void RenderMask(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionComponent caption,
        CaptionLayout layout,
        UIBox2i bounds)
    {
        var anchor = layout.Anchor - (Vector2) bounds.TopLeft;
        var shake = caption.Shake / layout.Scale;
        var wave = caption.LetterWave / layout.Scale;

        handle.RenderInRenderTarget(targets.Mask,
            () =>
            {
                handle.SetTransform(anchor, Angle.Zero, new Vector2(layout.Scale, layout.Scale));

                DrawSubject(handle, caption, layout, shake);
                DrawCaption(handle, caption, layout, shake, wave);

                handle.SetTransform(Matrix3x2.Identity);
            },
            Color.Transparent);
    }

    private static void DrawSubject(DrawingHandleScreen handle,
        CinematicCaptionComponent caption,
        CaptionLayout layout,
        float shake)
    {
        if (layout.SubjectFont is not { } font)
            return;

        var drift = MathF.Sin(layout.Time * caption.WaveSpeed * 0.6f - 1.3f) * shake * 0.5f;
        var x = -layout.SubjectWidth / 2f;

        for (var c = 0; c < caption.Subject.Length; c++)
        {
            var glyph = caption.Subject[c].ToString();
            handle.DrawString(font, new Vector2(x, layout.SubjectTop + drift), glyph, Color.White);

            x += handle.GetDimensions(font, glyph, 1f).X;
            if (c < caption.Subject.Length - 1)
                x += layout.SubjectTracking;
        }
    }

    private static void DrawCaption(DrawingHandleScreen handle,
        CinematicCaptionComponent caption,
        CaptionLayout layout,
        float shake,
        float wave)
    {
        var y = 0f;
        var written = 0;

        for (var i = 0; i < layout.Lines.Count; i++)
        {
            var line = layout.Lines[i];
            var x = -layout.Widths[i] / 2f;
            var drift = MathF.Sin(layout.Time * caption.WaveSpeed * 0.6f + i * 1.3f) * shake * 0.5f;

            for (var c = 0; c < line.Length; c++)
            {
                if (written >= layout.Shown)
                {
                    if (caption.Cursor.Length > 0)
                        handle.DrawString(layout.Font, new Vector2(x, y + drift), caption.Cursor, Color.White);

                    return;
                }

                var bob = wave <= 0f
                    ? 0f
                    : MathF.Sin(layout.Time * caption.WaveSpeed + c * 0.55f + i) * wave;

                var glyph = line[c].ToString();
                handle.DrawString(layout.Font, new Vector2(x, y + drift + bob), glyph, Color.White);

                written++;
                x += handle.GetDimensions(layout.Font, glyph, 1f).X;
                if (c < line.Length - 1)
                    x += layout.Tracking;
            }

            y += layout.LineStep;
        }
    }

    private static void BuildPyramid(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionComponent caption,
        ShaderInstance blur)
    {
        blur.SetParameter("spread", caption.BlurSpread);

        var source = targets.Mask.Texture;

        foreach (var level in targets.Levels)
        {
            BlurPass(handle, blur, source, level.Scratch, new Vector2(1f, 0f));
            BlurPass(handle, blur, level.Scratch.Texture, level.Blurred, new Vector2(0f, 1f));

            source = level.Blurred.Texture;
        }
    }

    private static void BlurPass(DrawingHandleScreen handle,
        ShaderInstance blur,
        Texture source,
        IRenderTexture target,
        Vector2 axis)
    {
        blur.SetParameter("axis", axis);

        handle.RenderInRenderTarget(target,
            () =>
            {
                handle.UseShader(blur);
                handle.DrawTextureRect(source, UIBox2.FromDimensions(Vector2.Zero, target.Size));
                handle.UseShader(null);
            },
            Color.Transparent);
    }

    /// <summary>
    /// Draws the letterforms on their own, for a caption whose aura is turned off.
    /// The mask already holds them at their final size, so it only needs tinting.
    /// </summary>
    private static void DrawPlain(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionComponent caption,
        CaptionLayout layout,
        UIBox2i bounds,
        float strength)
    {
        var color = caption.TextColor.WithAlpha(caption.TextColor.A * strength);

        handle.DrawTextureRectRegion(targets.Mask.Texture,
            layout.Block,
            MaskRegion(layout.Block, bounds),
            color);
    }

    /// <summary>
    /// Draws the aura, the scrim and the letterforms in one pass.
    /// </summary>
    private static void Composite(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionComponent caption,
        CaptionLayout layout,
        UIBox2i bounds,
        float strength,
        ShaderInstance composite)
    {
        var reach = layout.GlyphSize * caption.AuraScale;
        var (mid, midSigma) = PickLevel(targets, caption, reach * caption.BloomReachFraction);
        var (far, farSigma) = PickLevel(targets, caption, reach * caption.PressureReachFraction);

        composite.SetParameter("MID", mid.Blurred.Texture);
        composite.SetParameter("FAR", far.Blurred.Texture);
        composite.SetParameter("sigmaMid", midSigma);
        composite.SetParameter("sigmaFar", farSigma);
        composite.SetParameter("hotColor", caption.HotColor);
        composite.SetParameter("midColor", caption.MidColor);
        composite.SetParameter("deepColor", caption.DeepColor);
        composite.SetParameter("fillColor", caption.TextColor);
        composite.SetParameter("glyphSize", reach);
        composite.SetParameter("strength", strength);
        composite.SetParameter("animTime", layout.Time);
        composite.SetParameter("scrimAmount", caption.ScrimAmount);
        composite.SetParameter("waveSpeed", caption.WaveSpeed);
        composite.SetParameter("sweepPhase", layout.Age * 0.35f % 1f);
        composite.SetParameter("texelSize", new Vector2(1f / targets.Size.X, 1f / targets.Size.Y));
        composite.SetParameter("maskEdge", Math.Clamp(0.5f / layout.Scale, 0.04f, 0.5f));

        handle.UseShader(composite);
        handle.DrawTextureRectRegion(targets.Mask.Texture, layout.Block, MaskRegion(layout.Block, bounds));
        handle.UseShader(null);
    }

    private static UIBox2 MaskRegion(UIBox2 block, UIBox2i bounds)
        => new(block.Left - bounds.Left,
            block.Top - bounds.Top,
            block.Right - bounds.Left,
            block.Bottom - bounds.Top);

    private static (BlurLevel Level, float Sigma) PickLevel(CaptionTargets targets,
        CinematicCaptionComponent caption,
        float target)
    {
        var best = 0;
        var bestSigma = 0f;
        var bestError = float.MaxValue;
        var variance = 0f;

        for (var i = 0; i < targets.Levels.Length; i++)
        {
            var pass = caption.BlurSpread * caption.BlurPassSigma * (1 << (i + 1));
            variance += pass * pass;

            var sigma = MathF.Sqrt(variance);
            var error = MathF.Abs(MathF.Log(sigma / MathF.Max(target, 0.001f)));
            if (error >= bestError)
                continue;

            best = i;
            bestSigma = sigma;
            bestError = error;
        }

        return (targets.Levels[best], bestSigma);
    }

    private CaptionTargets EnsureTargets(Vector2i size, CinematicCaptionComponent caption)
    {
        var levelCount = caption.AuraEnabled ? Math.Max(caption.BlurLevelCount, 1) : 0;

        if (_targets is { } current
            && current.Size == size
            && (levelCount == 0 || current.Levels.Length == levelCount))
            return current;

        _targets?.Dispose();

        var sample = new TextureSampleParameters { Filter = true };
        var maskFormat = new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8);
        var blurFormat = new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba16F);
        var mask = _clyde.CreateRenderTarget(size, maskFormat, sample, "heretic-caption-mask");
        var levels = new BlurLevel[levelCount];

        for (var i = 0; i < levels.Length; i++)
        {
            var levelSize = Vector2i.ComponentMax(size / (1 << (i + 1)), Vector2i.One);

            levels[i] = new BlurLevel(
                _clyde.CreateRenderTarget(levelSize, blurFormat, sample, $"heretic-caption-scratch{i}"),
                _clyde.CreateRenderTarget(levelSize, blurFormat, sample, $"heretic-caption-blur{i}"));
        }

        _targets = new CaptionTargets(size, mask, levels);
        return _targets;
    }

    protected override void DisposeBehavior()
    {
        _targets?.Dispose();
        _targets = null;

        base.DisposeBehavior();
    }

    private sealed class CaptionTargets(Vector2i size, IRenderTexture mask, BlurLevel[] levels) : IDisposable
    {
        public readonly Vector2i Size = size;
        public readonly IRenderTexture Mask = mask;
        public readonly BlurLevel[] Levels = levels;

        public void Dispose()
        {
            Mask.Dispose();

            foreach (var level in Levels)
                level.Dispose();
        }
    }

    private sealed class BlurLevel(IRenderTexture scratch, IRenderTexture blurred) : IDisposable
    {
        public readonly IRenderTexture Scratch = scratch;
        public readonly IRenderTexture Blurred = blurred;

        public void Dispose()
        {
            Scratch.Dispose();
            Blurred.Dispose();
        }
    }

    private sealed class CaptionLayout
    {
        public readonly List<string> Lines = new();
        public readonly List<float> Widths = new();
        public VectorFont Font = default!;
        public VectorFont? SubjectFont;
        public float Scale;
        public float Tracking;
        public float SubjectTracking;
        public float LineStep;
        public float SubjectWidth;
        public float SubjectTop;
        public Vector2 Anchor;
        public float GlyphSize;
        public int Shown;
        public float Age;
        public float Time;
        public UIBox2 Block;
    }
}
