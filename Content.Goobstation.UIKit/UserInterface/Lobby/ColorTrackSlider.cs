using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class ColorTrackSlider : Slider
{
    private const int Steps = 48;

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
        var top = size.Y * 0.25f;
        var bottom = size.Y * 0.75f;
        var step = size.X / (float) Steps;

        for (var i = 0; i < Steps; i++)
        {
            var value = MinValue + (MaxValue - MinValue) * ((i + 0.5f) / Steps);
            handle.DrawRect(new UIBox2(i * step, top, (i + 1) * step + 1, bottom), colorAt(value));
        }

        handle.DrawRect(new UIBox2(0, top, size.X, bottom), HullPanel.EdgeColor, false);
    }
}
