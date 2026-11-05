using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class ColorTrackSlider : Slider
{
    private const int Steps = 48;
    private const float FrameInset = 5;

    public Func<float, Color>? ColorAt { get; set; }

    public ColorTrackSlider()
    {
        StyleClasses.Add(HullStyleClasses.ColorTrack);
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (ColorAt is not { } colorAt)
            return;

        var size = PixelSize;
        var inset = FrameInset * UIScale;
        var top = inset;
        var bottom = size.Y - inset;
        var step = (size.X - inset * 2) / Steps;

        for (var i = 0; i < Steps; i++)
        {
            var value = MinValue + (MaxValue - MinValue) * ((i + 0.5f) / Steps);
            handle.DrawRect(new UIBox2(inset + i * step, top, inset + (i + 1) * step + 1, bottom), colorAt(value));
        }
    }
}
