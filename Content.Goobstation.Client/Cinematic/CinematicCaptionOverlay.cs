using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Cinematic;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Graphics;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Cinematic;

/// <summary>
/// Displays text that gets written in on the users screen.
/// </summary>
public sealed partial class CinematicCaptionOverlay : Overlay
{
    // Font sizes are authored against a 1080p viewport. Glyph sheets above 248px break the engine, below 8px are illegible.
    private const float ReferenceViewportHeight = 1080f;
    private const int MaxGlyphSheetExtent = 248;
    private const int MinGlyphRasterSize = 8;
    private const int DrawOrder = 205;
    private const int BlurLevels = 6;
    private const float AuraPadding = 2.6f;
    private const float ThrobFrequency = 2.6f;
    private const float DriftFrequency = 0.6f;
    private const float DriftAmplitude = 0.5f;
    private const float DriftLinePhase = 1.3f;
    private const float BobGlyphPhase = 0.55f;
    private const float SweepRate = 0.35f;

    private static readonly Vector4 AlphaChannel = new(0f, 0f, 0f, 1f);
    private static readonly Vector4 RedChannel = new(1f, 0f, 0f, 0f);

    [Dependency] private IClyde _clyde = default!;
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IGameTiming _timing = default!;

    // The live caption plus any still dissolving. Owned here so the system can construct the overlay inline.
    public readonly List<CinematicCaptionSystem.Caption> Captions = new();
    private readonly Dictionary<CinematicCaptionSystem.Caption, CaptionLayout> _layouts = new();
    private readonly Dictionary<(ResPath Path, int Size), VectorFont> _fonts = new();
    private CaptionTargets? _targets;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace;

    public CinematicCaptionOverlay()
    {
        IoCManager.InjectDependencies(this);
        ZIndex = DrawOrder;
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
        => Captions.Count > 0 && base.BeforeDraw(in args);

    protected override void Draw(in OverlayDrawArgs args)
    {
        var bounds = args.ViewportBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        var handle = args.ScreenHandle;
        var targets = EnsureTargets(bounds.Size);
        var now = (float) _timing.CurTime.TotalSeconds;

        if (_layouts.Count > Captions.Count)
            foreach (var stale in _layouts.Keys.Where(caption => !Captions.Contains(caption)).ToArray())
                _layouts.Remove(stale);

        foreach (var caption in Captions)
        {
            if (caption.Text.Length == 0)
                continue;

            var style = caption.Style;
            var layout = GetLayout(handle, caption, bounds);
            if (layout.GlyphCount == 0)
                continue;

            var frame = Animate(caption, layout, bounds, now);
            var ignite = style.IgniteTime > 0f ? Math.Clamp(frame.Age / style.IgniteTime, 0f, 1f) : 1f;
            var strength = MathF.Min(ignite, caption.Strength) * caption.Fade;
            if (strength <= 0f)
                continue;

            RenderMask(handle, targets, caption, layout, frame, bounds);

            if (!style.AuraEnabled)
            {
                DrawPlain(handle, targets, style, frame, bounds, strength);
                continue;
            }

            if (caption.AuraShader is not { } aura || caption.BlurShader is not { } blur)
                continue;

            BuildPyramid(handle, targets, style, blur);
            Composite(handle, targets, style, frame, bounds, strength, aura);
        }
    }

    private CaptionLayout GetLayout(DrawingHandleScreen handle, CinematicCaptionSystem.Caption caption, UIBox2i bounds)
    {
        if (!_layouts.TryGetValue(caption, out var layout))
            _layouts[caption] = layout = new CaptionLayout();

        if (layout.Matches(caption, bounds.Size))
            return layout;

        var style = caption.Style;
        var viewportScale = bounds.Height / ReferenceViewportHeight;
        var font = RasterFont(style.FontPath, style.FontSize, viewportScale);
        var subjectFont = caption.Subject.Length > 0
            ? RasterFont(style.SubjectFontPath ?? style.FontPath, style.FontSize, viewportScale * style.SubjectScale)
            : null;

        var restScale = MathF.Max(style.FontSize * viewportScale, 1f) / font.Size;
        var maxWidth = bounds.Width * style.MaxWidthFraction / restScale;

        layout.Rebuild(handle, caption, bounds.Size, font, subjectFont, maxWidth);
        return layout;
    }

    private VectorFont RasterFont(ResPath path, int fontSize, float scale)
    {
        var size = Math.Max(MinGlyphRasterSize, (int) (fontSize * scale));
        var font = GetFont(path, size);
        var height = font.GetHeight(1f);
        if (height <= MaxGlyphSheetExtent)
            return font;

        return GetFont(path, Math.Max(MinGlyphRasterSize, size * MaxGlyphSheetExtent / height));
    }

    private VectorFont GetFont(ResPath path, int size)
    {
        var key = (path, size);
        if (!_fonts.TryGetValue(key, out var font))
            _fonts[key] = font = new VectorFont(_cache.GetResource<FontResource>(path), size);

        return font;
    }

    private static CaptionFrame Animate(CinematicCaptionSystem.Caption caption, CaptionLayout layout, UIBox2i bounds, float now)
    {
        var style = caption.Style;
        var age = Step(caption.Age, style.StepRate);
        var time = Step(now, style.StepRate);

        var size = style.FontSize * bounds.Height / ReferenceViewportHeight;
        if (style.SlamScale > 0f)
            size *= 1f + style.SlamScale * MathF.Exp(-age * style.SlamDecay);
        if (style.Kick > 0f && age >= style.KickTime)
            size *= 1f + style.Kick * MathF.Exp(-(age - style.KickTime) * style.KickDecay);
        if (style.Throb > 0f)
            size *= 1f + style.Throb * MathF.Sin(age * ThrobFrequency);

        var scale = MathF.Max(size, 1f) / layout.Font.Size;
        var glyphSize = layout.Font.Size * scale;
        var lineHeight = layout.LineStep * scale;
        var blockHeight = layout.Lines.Count * lineHeight;
        var subjectHeight = -layout.SubjectTop * scale;

        var centerX = bounds.Left + bounds.Width / 2f;
        var top = bounds.Top + bounds.Height * style.VerticalPosition - blockHeight / 2f;
        var lowest = bounds.Bottom - blockHeight - lineHeight * 0.5f;
        var highest = bounds.Top + lineHeight * 0.5f + subjectHeight;
        if (lowest > highest)
            top = Math.Clamp(top, highest, lowest);

        var half = layout.Widest * scale / 2f;
        var pad = glyphSize * AuraPadding * style.AuraScale;
        var block = new UIBox2(
            MathF.Max(centerX - half - pad, bounds.Left),
            MathF.Max(top - subjectHeight - pad, bounds.Top),
            MathF.Min(centerX + half + pad, bounds.Right),
            MathF.Min(top + blockHeight + pad, bounds.Bottom));

        return new CaptionFrame(scale, glyphSize, new Vector2(centerX, top), block, layout.CountShown(caption.Progress), age, time);
    }

    private static float Step(float time, float rate)
        => rate > 0f ? MathF.Floor(time * rate) / rate : time;

    private static float Drift(float time, float waveSpeed, float shake, float phase)
        => MathF.Sin(time * waveSpeed * DriftFrequency + phase) * shake * DriftAmplitude;

    private static void RenderMask(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionSystem.Caption caption,
        CaptionLayout layout,
        CaptionFrame frame,
        UIBox2i bounds)
    {
        var anchor = frame.Anchor - (Vector2) bounds.TopLeft;
        var shake = caption.Style.Shake / frame.Scale;
        var wave = caption.Style.LetterWave / frame.Scale;

        handle.RenderInRenderTarget(targets.Mask,
            () =>
            {
                handle.SetTransform(anchor, Angle.Zero, new Vector2(frame.Scale, frame.Scale));

                DrawSubject(handle, caption, layout, frame, shake);
                DrawCaption(handle, caption, layout, frame, shake, wave);

                handle.SetTransform(Matrix3x2.Identity);
            },
            Color.Transparent);
    }

    private static void DrawSubject(DrawingHandleScreen handle,
        CinematicCaptionSystem.Caption caption,
        CaptionLayout layout,
        CaptionFrame frame,
        float shake)
    {
        if (layout.SubjectFont is not { } font || caption.Subject.Length == 0)
            return;

        var drift = Drift(frame.Time, caption.Style.WaveSpeed, shake, -DriftLinePhase);
        var x = -layout.SubjectWidth / 2f;

        for (var c = 0; c < caption.Subject.Length; c++)
        {
            handle.DrawString(font, new Vector2(x, layout.SubjectTop + drift), caption.Subject.AsSpan(c, 1), 1f, Color.White);
            x += layout.SubjectAdvances[c] + layout.SubjectTracking;
        }
    }

    private static void DrawCaption(DrawingHandleScreen handle,
        CinematicCaptionSystem.Caption caption,
        CaptionLayout layout,
        CaptionFrame frame,
        float shake,
        float wave)
    {
        var style = caption.Style;
        var y = 0f;
        var written = 0;

        for (var i = 0; i < layout.Lines.Count; i++)
        {
            var line = layout.Lines[i];
            var advances = layout.Advances[i];
            var x = -layout.LineWidths[i] / 2f;
            var drift = Drift(frame.Time, style.WaveSpeed, shake, i * DriftLinePhase);

            for (var c = 0; c < line.Length; c++)
            {
                if (written >= frame.Shown)
                {
                    if (style.Cursor.Length > 0)
                        handle.DrawString(layout.Font, new Vector2(x, y + drift), style.Cursor, Color.White);

                    return;
                }

                var bob = wave <= 0f
                    ? 0f
                    : MathF.Sin(frame.Time * style.WaveSpeed + c * BobGlyphPhase + i) * wave;

                handle.DrawString(layout.Font, new Vector2(x, y + drift + bob), line.AsSpan(c, 1), 1f, Color.White);

                written++;
                x += advances[c] + layout.Tracking;
            }

            y += layout.LineStep;
        }
    }

    private static void BuildPyramid(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionStylePrototype style,
        ShaderInstance blur)
    {
        blur.SetParameter("spread", style.BlurSpread);

        var source = targets.Mask.Texture;
        var channel = AlphaChannel;

        foreach (var level in targets.Levels)
        {
            BlurPass(handle, blur, source, channel, level.Scratch, new Vector2(1f, 0f));
            BlurPass(handle, blur, level.Scratch.Texture, RedChannel, level.Blurred, new Vector2(0f, 1f));

            source = level.Blurred.Texture;
            channel = RedChannel;
        }
    }

    private static void BlurPass(DrawingHandleScreen handle,
        ShaderInstance blur,
        Texture source,
        Vector4 channel,
        IRenderTexture target,
        Vector2 axis)
    {
        blur.SetParameter("axis", axis);
        blur.SetParameter("channel", channel);

        handle.RenderInRenderTarget(target,
            () =>
            {
                handle.UseShader(blur);
                handle.DrawTextureRect(source, UIBox2.FromDimensions(Vector2.Zero, target.Size));
                handle.UseShader(null);
            },
            Color.Transparent);
    }

    private static void DrawPlain(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionStylePrototype style,
        CaptionFrame frame,
        UIBox2i bounds,
        float strength)
    {
        var color = style.TextColor.WithAlpha(style.TextColor.A * strength);

        handle.DrawTextureRectRegion(targets.Mask.Texture,
            frame.Block,
            MaskRegion(frame.Block, bounds),
            color);
    }

    private static void Composite(DrawingHandleScreen handle,
        CaptionTargets targets,
        CinematicCaptionStylePrototype style,
        CaptionFrame frame,
        UIBox2i bounds,
        float strength,
        ShaderInstance composite)
    {
        var reach = frame.GlyphSize * style.AuraScale;
        var (mid, midSigma) = PickLevel(targets, style, reach * style.BloomReachFraction);
        var (far, farSigma) = PickLevel(targets, style, reach * style.PressureReachFraction);

        composite.SetParameter("MID", mid.Blurred.Texture);
        composite.SetParameter("FAR", far.Blurred.Texture);
        composite.SetParameter("sigmaMid", midSigma);
        composite.SetParameter("sigmaFar", farSigma);
        composite.SetParameter("hotColor", style.HotColor);
        composite.SetParameter("midColor", style.MidColor);
        composite.SetParameter("deepColor", style.DeepColor);
        composite.SetParameter("fillColor", style.TextColor);
        composite.SetParameter("glyphSize", reach);
        composite.SetParameter("strength", strength);
        composite.SetParameter("animTime", frame.Time);
        composite.SetParameter("scrimAmount", style.ScrimAmount);
        composite.SetParameter("waveSpeed", style.WaveSpeed);
        composite.SetParameter("sweepPhase", frame.Age * SweepRate % 1f);
        composite.SetParameter("texelSize", new Vector2(1f / targets.Size.X, 1f / targets.Size.Y));
        composite.SetParameter("maskEdge", Math.Clamp(0.5f / frame.Scale, 0.04f, 0.5f));

        handle.UseShader(composite);
        handle.DrawTextureRectRegion(targets.Mask.Texture, frame.Block, MaskRegion(frame.Block, bounds));
        handle.UseShader(null);
    }

    private static UIBox2 MaskRegion(UIBox2 block, UIBox2i bounds)
        => new(block.Left - bounds.Left,
            block.Top - bounds.Top,
            block.Right - bounds.Left,
            block.Bottom - bounds.Top);

    private static (BlurLevel Level, float Sigma) PickLevel(CaptionTargets targets,
        CinematicCaptionStylePrototype style,
        float target)
    {
        var best = 0;
        var bestSigma = 0f;
        var bestError = float.MaxValue;
        var variance = 0f;

        for (var i = 0; i < targets.Levels.Length; i++)
        {
            var pass = style.BlurSpread * style.BlurPassSigma * (1 << (i + 1));
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

    private CaptionTargets EnsureTargets(Vector2i size)
    {
        if (_targets is { } current && current.Size == size)
            return current;

        ReleaseTargets();
        _fonts.Clear();

        _targets = new CaptionTargets(_clyde, size, BlurLevels);
        return _targets;
    }

    public void ReleaseTargets()
    {
        _targets?.Dispose();
        _targets = null;
    }

    protected override void DisposeBehavior()
    {
        ReleaseTargets();
        _fonts.Clear();
        _layouts.Clear();

        base.DisposeBehavior();
    }

    private readonly record struct CaptionFrame(float Scale,
        float GlyphSize,
        Vector2 Anchor,
        UIBox2 Block,
        int Shown,
        float Age,
        float Time);

    private sealed class CaptionTargets : IDisposable
    {
        public readonly Vector2i Size;
        public readonly IRenderTexture Mask;
        public readonly BlurLevel[] Levels;

        public CaptionTargets(IClyde clyde, Vector2i size, int levelCount)
        {
            var sample = new TextureSampleParameters { Filter = true };
            var maskFormat = new RenderTargetFormatParameters(RenderTargetColorFormat.Rgba8);
            var blurFormat = new RenderTargetFormatParameters(RenderTargetColorFormat.R8);

            Size = size;
            Mask = clyde.CreateRenderTarget(size, maskFormat, sample, "cinematic-caption-mask");
            Levels = new BlurLevel[levelCount];

            for (var i = 0; i < Levels.Length; i++)
            {
                var levelSize = Vector2i.ComponentMax(size / (1 << (i + 1)), Vector2i.One);

                Levels[i] = new BlurLevel(
                    clyde.CreateRenderTarget(levelSize, blurFormat, sample, $"cinematic-caption-scratch{i}"),
                    clyde.CreateRenderTarget(levelSize, blurFormat, sample, $"cinematic-caption-blur{i}"));
            }
        }

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
        private const string PauseGlyphs = ".,!?;:…";

        private string? _text;
        private string? _subject;
        private CinematicCaptionStylePrototype? _style;
        private Vector2i _viewport;

        public VectorFont Font = default!;
        public VectorFont? SubjectFont;
        public readonly List<string> Lines = new();
        public readonly List<float> LineWidths = new();
        public readonly List<float[]> Advances = new();
        public float[] SubjectAdvances = Array.Empty<float>();
        public float Widest;
        public float Tracking;
        public float LineStep;
        public float SubjectTracking;
        public float SubjectWidth;
        public float SubjectTop;
        public int GlyphCount;
        private readonly List<float> _revealAt = new();
        private float _revealTotal;

        public bool Matches(CinematicCaptionSystem.Caption caption, Vector2i viewport)
            => ReferenceEquals(_style, caption.Style)
               && _viewport == viewport
               && _text == caption.Text
               && _subject == caption.Subject;

        public void Rebuild(DrawingHandleScreen handle,
            CinematicCaptionSystem.Caption caption,
            Vector2i viewport,
            VectorFont font,
            VectorFont? subjectFont,
            float maxWidth)
        {
            var style = caption.Style;

            _text = caption.Text;
            _subject = caption.Subject;
            _style = style;
            _viewport = viewport;

            Font = font;
            SubjectFont = subjectFont;
            Tracking = style.Tracking * font.Size;
            LineStep = font.Size * style.LineSpacing;

            Wrap(handle, caption.Text, maxWidth);

            LineWidths.Clear();
            Advances.Clear();
            _revealAt.Clear();
            _revealTotal = 0f;
            Widest = 0f;
            GlyphCount = 0;

            var pause = MathF.Max(1f, style.PunctuationPause);

            foreach (var line in Lines)
            {
                var advances = MeasureGlyphs(handle, font, line);
                var width = advances.Sum() + Tracking * Math.Max(0, line.Length - 1);

                Advances.Add(advances);
                LineWidths.Add(width);
                Widest = MathF.Max(Widest, width);
                GlyphCount += line.Length;

                foreach (var glyph in line)
                {
                    _revealAt.Add(_revealTotal);
                    _revealTotal += PauseGlyphs.Contains(glyph) ? pause : 1f;
                }
            }

            SubjectAdvances = Array.Empty<float>();
            SubjectTracking = 0f;
            SubjectWidth = 0f;
            SubjectTop = 0f;

            if (subjectFont == null || caption.Subject.Length == 0)
                return;

            SubjectAdvances = MeasureGlyphs(handle, subjectFont, caption.Subject);
            SubjectTracking = style.SubjectTracking * subjectFont.Size;
            SubjectWidth = SubjectAdvances.Sum() + SubjectTracking * Math.Max(0, caption.Subject.Length - 1);
            SubjectTop = -subjectFont.Size * style.LineSpacing * (1f + style.SubjectGap);
            Widest = MathF.Max(Widest, SubjectWidth);
        }

        public int CountShown(float progress)
        {
            if (progress >= 1f)
                return GlyphCount;

            var target = progress * _revealTotal;
            var shown = 0;

            while (shown < GlyphCount && _revealAt[shown] < target)
                shown++;

            return shown;
        }

        private void Wrap(DrawingHandleScreen handle, string text, float maxWidth)
        {
            Lines.Clear();

            foreach (var paragraph in text.Split('\n'))
            {
                var line = string.Empty;

                foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    var candidate = line.Length == 0 ? word : line + " " + word;

                    if (line.Length > 0 && Measure(handle, Font, candidate, Tracking) > maxWidth)
                    {
                        Lines.Add(line);
                        candidate = word;
                    }

                    line = BreakLongWord(handle, candidate, maxWidth);
                }

                Lines.Add(line);
            }
        }

        private string BreakLongWord(DrawingHandleScreen handle, string word, float maxWidth)
        {
            while (word.Length > 1 && Measure(handle, Font, word, Tracking) > maxWidth)
            {
                var fit = word.Length - 1;
                while (fit > 1 && Measure(handle, Font, word.AsSpan(0, fit), Tracking) > maxWidth)
                    fit--;

                Lines.Add(word[..fit]);
                word = word[fit..];
            }

            return word;
        }

        private static float Measure(DrawingHandleScreen handle, VectorFont font, ReadOnlySpan<char> text, float tracking)
            => handle.GetDimensions(font, text, 1f).X + tracking * Math.Max(0, text.Length - 1);

        private static float[] MeasureGlyphs(DrawingHandleScreen handle, VectorFont font, string text)
        {
            var advances = new float[text.Length];
            for (var i = 0; i < text.Length; i++)
                advances[i] = handle.GetDimensions(font, text.AsSpan(i, 1), 1f).X;

            return advances;
        }
    }
}
