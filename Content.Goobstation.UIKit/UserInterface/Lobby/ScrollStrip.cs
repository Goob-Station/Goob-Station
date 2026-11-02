using System.Numerics;
using Robust.Client.UserInterface.Controls;

namespace Content.Goobstation.UIKit.UserInterface.Lobby;

public sealed class ScrollStrip : ScrollContainer
{
    public ScrollStrip()
    {
        HScrollEnabled = true;
        VScrollEnabled = false;
        ReturnMeasure = true;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        return new Vector2(0, base.MeasureOverride(availableSize).Y);
    }
}
