using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class ColorSwatch : Button
{
    public Color Color { get; }

    public ColorSwatch(Color color)
    {
        Color = color;
        ToggleMode = true;
        SetSize = new Vector2(24, 24);
        StyleClasses.Add(HullStyleClasses.Slot);
        ToolTip = color.ToHex();

        AddChild(new PanelContainer
        {
            PanelOverride = new StyleBoxFlat { BackgroundColor = color },
            Margin = new Thickness(2),
            MouseFilter = MouseFilterMode.Ignore,
        });
    }

    public bool Matches(Color color)
    {
        return Color.ToHex() == color.ToHex();
    }
}
