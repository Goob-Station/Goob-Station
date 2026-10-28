using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Chat.UI;

public static class RunechatStyle
{
    public const string FontPath = "/Fonts/_ES/Grand9KPixelRegular.ttf";
    public const string WhisperFontPath = "/Fonts/_ES/Wormtown9k-Small.ttf";
    public const int MinFontSize = 6;
    public const int MaxFontSize = 24;
    private const float DesignPixelsPerEm = 8f;

    private static readonly Dictionary<(string, int), Font> Fonts = new();
    private static readonly Dictionary<(string, int), Stylesheet> Sheets = new();

    public static Font GetFont(string path, int size)
    {
        if (Fonts.TryGetValue((path, size), out var font))
            return font;

        var cache = IoCManager.Resolve<IResourceCache>();
        font = new VectorFont(cache.GetResource<FontResource>(path), size);
        Fonts[(path, size)] = font;
        return font;
    }

    public static Stylesheet GetStylesheet(string path, int size)
    {
        if (Sheets.TryGetValue((path, size), out var sheet))
            return sheet;

        sheet = new Stylesheet(new[]
        {
            new StyleRule(
                new SelectorElement(typeof(RichTextLabel), null, null, null),
                new[] { new StyleProperty(Label.StylePropertyFont, GetFont(path, size)) }),
        });
        Sheets[(path, size)] = sheet;
        return sheet;
    }

    public static int ScaleSize(int baseSize, int fontSize)
    {
        return Math.Max(1, (int) MathF.Round(baseSize * fontSize / 12f));
    }

    public static int GetLoudSize(int fontSize)
    {
        return ScaleSize(18, fontSize);
    }

    public static float GetOutline(int pointSize, float scale)
    {
        return pointSize * 96f / 72f / DesignPixelsPerEm * scale;
    }
}
