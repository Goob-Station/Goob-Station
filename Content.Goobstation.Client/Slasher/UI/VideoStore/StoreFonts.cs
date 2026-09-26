using System.Text;
using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Goobstation.Client.Slasher.UI.VideoStore;

public static class StoreFonts
{
    private const string Symbols = "/Fonts/NotoSans/NotoSansSymbols-Regular.ttf";
    private const string Symbols2 = "/Fonts/NotoSans/NotoSansSymbols2-Regular.ttf";
    private const string Fallback = "/Fonts/NotoSansDisplay/NotoSansDisplay-Bold.ttf";

    public static readonly string[] DripPaths =
    {
        "/Fonts/_Goobstation/Nosifer/Nosifer-Regular.ttf", Fallback, Symbols2, Symbols,
    };

    public static readonly string[] SignPaths =
    {
        "/Fonts/_Goobstation/BebasNeue/BebasNeue-Regular.ttf", Fallback, Symbols2, Symbols,
    };

    public static readonly string[] TermPaths =
    {
        "/Fonts/_Goobstation/VT323/VT323-Regular.ttf", Fallback, Symbols2, Symbols,
    };

    public static readonly string[] StencilPaths =
    {
        "/Fonts/_Goobstation/BlackOpsOne/BlackOpsOne-Regular.ttf", Fallback, Symbols2, Symbols,
    };

    private static readonly (int Value, string Glyph)[] RomanGlyphs =
    {
        (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"), (100, "C"), (90, "XC"), (50, "L"),
        (40, "XL"), (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I"),
    };

    public static Font Drip(IResourceCache cache, int size)
    {
        return cache.GetFont(DripPaths, size);
    }

    public static Font Sign(IResourceCache cache, int size)
    {
        return cache.GetFont(SignPaths, size);
    }

    public static Font Term(IResourceCache cache, int size)
    {
        return cache.GetFont(TermPaths, size);
    }

    public static Font Stencil(IResourceCache cache, int size)
    {
        return cache.GetFont(StencilPaths, size);
    }

    public static float MeasureWidth(Font font, string text, float scale)
    {
        var width = 0f;
        foreach (var rune in text.EnumerateRunes())
            if (font.TryGetCharMetrics(rune, scale, out var metrics))
                width += metrics.Advance;

        return width;
    }

    public static List<string> Wrap(Font font, string text, float maxWidth, float scale)
    {
        var lines = new List<string>();
        var spaceWidth = MeasureWidth(font, " ", scale);

        foreach (var paragraph in text.Split('\n'))
        {
            var line = string.Empty;
            var lineWidth = 0f;

            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var wordWidth = MeasureWidth(font, word, scale);
                var candidate = line.Length == 0 ? wordWidth : lineWidth + spaceWidth + wordWidth;

                if (line.Length > 0 && candidate > maxWidth)
                {
                    lines.Add(line);
                    line = word;
                    lineWidth = wordWidth;
                    continue;
                }

                line = line.Length == 0 ? word : line + " " + word;
                lineWidth = candidate;
            }

            lines.Add(line);
        }

        return lines;
    }

    public static string Roman(int number)
    {
        var result = new StringBuilder();
        foreach (var (value, glyph) in RomanGlyphs)
            while (number >= value)
            {
                result.Append(glyph);
                number -= value;
            }

        return result.ToString();
    }

    public static string Part(int part)
    {
        return Loc.GetString("slasher-kit-store-part", ("part", Roman(part)));
    }
}
