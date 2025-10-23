using System.Diagnostics.CodeAnalysis;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed class TextureTag : BaseTextureTag
{
    public string Name => "tex";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Attributes.TryGetValue("path", out var rawPath))
            return false;

        if (!node.Attributes.TryGetValue("scale", out var scale) || !scale.TryGetLong(out var scaleValue))
        {
            scaleValue = 1;
        }

        if (!TryDrawIcon(rawPath.ToString(), scaleValue.Value, out var texture))
            return false;

        control = texture;
        return true;
    }
}
