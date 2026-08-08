using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Maths;

namespace Content.Goobstation.Client.Slasher.UI;

public sealed class TierScrollContainer : ScrollContainer
{
    public TierScrollContainer()
    {
        foreach (var child in Children)
            if (child is VScrollBar vbar)
            {
                vbar.Modulate = Color.Transparent;
                vbar.MouseFilter = MouseFilterMode.Ignore;
            }
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        if (!HScrollEnabled)
            return;

        var delta = args.Delta.X != 0f ? args.Delta.X : -args.Delta.Y;
        HScrollTarget += delta * ScrollSpeedX;
        args.Handle();
    }
}
