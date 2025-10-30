using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Goobstation.UIKit.UserInterface.Controls;

public sealed class TooltipTextureRect : TextureRect
{
    public string? TooltipText;
    public Vector2 Offset;

    public TooltipTextureRect(string? tooltip, Vector2 offset)
    {
        TooltipText = tooltip;
        Offset = offset;
        TooltipSupplier = Supply;
        MouseFilter = MouseFilterMode.Stop;
        HorizontalAlignment = HAlignment.Center;
        VerticalAlignment = VAlignment.Center;
    }

    private Control? Supply(Control? sender)
    {
        if (TooltipText == null)
            return null;

        return new Tooltip
        {
            Text = TooltipText,
        };
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        var transform = Matrix3Helpers.CreateTranslation(GlobalPixelPosition + Offset * UIScale);
        handle.SetTransform(transform);
        base.Draw(handle);
        handle.SetTransform(Matrix3x2.Identity);
    }

    protected override bool HasPoint(Vector2 point)
    {
        return base.HasPoint(point - Offset * UIScale);
    }
}
