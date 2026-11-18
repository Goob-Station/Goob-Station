using System.Numerics;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class ColorSwatch : TintButton
{
    public Color Color { get; }

    public ColorSwatch(Color color)
    {
        Color = color;
        ToggleMode = true;
        SetSize = new Vector2(24, 24);
        StyleClasses.Add(HullStyleClasses.Plate);
        ToolTip = color.ToHex();
        Tint = color;
    }

    public bool Matches(Color color)
    {
        return Color.ToHex() == color.ToHex();
    }
}
