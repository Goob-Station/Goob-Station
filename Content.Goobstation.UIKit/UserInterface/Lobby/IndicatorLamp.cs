using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class IndicatorLamp : Control
{
    public Color LampColor { get; set; } = HazardStripe.StripeColor;

    public IndicatorLamp()
    {
        SetSize = new Vector2(8, 8);
        VerticalAlignment = VAlignment.Center;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var size = PixelSize;
        handle.DrawCircle(new Vector2(size.X / 2f, size.Y / 2f), MathF.Min(size.X, size.Y) / 2f, LampColor);
    }
}
