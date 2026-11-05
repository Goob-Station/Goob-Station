using System.Globalization;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.News.Tags;

public static class NewsTagArgs
{
    public static float Float(string value, float fallback = 1f)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback;
    }

    public static Thickness Margin(Dictionary<string, string> args)
    {
        return args.TryGetValue("Margin", out var margin) ? new Thickness(Float(margin, 0f)) : default;
    }

    public static Control.HAlignment HAlign(Dictionary<string, string> args, Control.HAlignment fallback)
    {
        return args.TryGetValue("HorizontalAlignment", out var align) && Enum.TryParse<Control.HAlignment>(align, out var parsed)
            ? parsed
            : fallback;
    }
}
