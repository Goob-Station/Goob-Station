using System.Linq;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Random;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

public sealed class NeonSign
{
    [Dependency] private readonly IResourceCache _cache = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private const float BreathRate = 2.1f;
    private const float Bloom = 34f;
    private const float BloomAlpha = 0.55f;
    private const float CoreWhiteness = 0.45f;

    private static readonly (float Radius, float Alpha)[] Halo = { (6f, 0.14f), (3f, 0.3f) };

    private static readonly Vector2[] HaloDirections = Enumerable.Range(0, 8)
        .Select(i => new Vector2(MathF.Cos(i * MathF.PI / 4f), MathF.Sin(i * MathF.PI / 4f)))
        .ToArray();

    private readonly Font _font;
    private readonly Texture _glow;

    private string _word = string.Empty;
    private Color _wordColor;
    private string _accent = string.Empty;
    private Color _accentColor;

    private float _time;
    private float _brightness = 1f;
    private float _nextFlicker = 2.5f;
    private float _flickerLeft;
    private float _flickerLevel;

    public NeonSign()
    {
        IoCManager.InjectDependencies(this);
        _font = StoreFonts.Sign(_cache, 66);
        _glow = StoreTextures.Get(_cache, "glow");
    }

    public void Set(string word, Color wordColor, string accent, Color accentColor)
    {
        _word = word;
        _wordColor = wordColor;
        _accent = accent;
        _accentColor = accentColor;
    }

    public void Update(float dt)
    {
        _time += dt;
        _brightness = 0.94f + 0.06f * MathF.Sin(_time * BreathRate);

        if (_flickerLeft > 0f)
        {
            _flickerLeft -= dt;
            _brightness *= _flickerLevel;
            return;
        }

        _nextFlicker -= dt;
        if (_nextFlicker > 0f)
            return;

        _flickerLeft = _random.NextFloat(0.04f, 0.14f);
        _flickerLevel = _random.NextFloat(0.45f, 0.8f);
        _nextFlicker = _random.NextFloat(1.2f, 5.5f);
    }

    public void DrawAt(DrawingHandleScreen handle, Vector2 center, float scale)
    {
        var wordWidth = StoreFonts.MeasureWidth(_font, _word, scale);
        var accentWidth = StoreFonts.MeasureWidth(_font, _accent, scale);
        var gap = _accent.Length > 0 ? StoreFonts.MeasureWidth(_font, " ", scale) : 0f;

        var left = center.X - (wordWidth + gap + accentWidth) * 0.5f;
        var top = center.Y - _font.GetHeight(scale) * 0.5f;

        DrawTube(handle, _word, _wordColor, new Vector2(left, top), wordWidth, scale);
        DrawTube(handle, _accent, _accentColor, new Vector2(left + wordWidth + gap, top), accentWidth, scale);
    }

    private void DrawTube(DrawingHandleScreen handle, string text, Color color, Vector2 pos, float width, float scale)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var bloom = Bloom * scale;
        var bloomBox = new UIBox2(
            pos.X - bloom,
            pos.Y - bloom,
            pos.X + width + bloom,
            pos.Y + _font.GetHeight(scale) + bloom);
        handle.DrawTextureRect(_glow, bloomBox, color.WithAlpha(BloomAlpha * _brightness));

        foreach (var (radius, alpha) in Halo)
        {
            var haloColor = color.WithAlpha(alpha * _brightness);
            foreach (var direction in HaloDirections)
                handle.DrawString(_font, pos + direction * radius * scale, text, scale, haloColor);
        }

        var core = Color.InterpolateBetween(color, Color.White, CoreWhiteness);
        handle.DrawString(_font, pos, text, scale, core.WithAlpha(_brightness));
    }
}
