using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class HazardStripe : Control
{
    public static readonly Color StripeColor = Color.FromHex("#e0a722");
    public static readonly Color GapColor = Color.FromHex("#17181a");

    private readonly Vector2[] _quad = new Vector2[6];

    public float StripeWidth { get; set; } = 7f;

    public HazardStripe()
    {
        RectClipContent = true;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        var size = PixelSize;
        handle.DrawRect(UIBox2.FromDimensions(Vector2.Zero, size), GapColor);

        var width = StripeWidth * UIScale;
        var height = (float) size.Y;

        for (var x = -height; x < size.X + height; x += width * 2)
        {
            var bottomLeft = new Vector2(x, height);
            var bottomRight = new Vector2(x + width, height);
            var topRight = new Vector2(x + width + height, 0);
            var topLeft = new Vector2(x + height, 0);

            _quad[0] = bottomLeft;
            _quad[1] = bottomRight;
            _quad[2] = topRight;
            _quad[3] = bottomLeft;
            _quad[4] = topRight;
            _quad[5] = topLeft;
            handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, _quad, StripeColor);
        }
    }
}
