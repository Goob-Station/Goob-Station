using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.UIKit.UserInterface.Chat;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.Chat;

public sealed class TextShaderTag : IMarkupTagHandler
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IResourceCache _cache = default!;

    public string Name => "textshader";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Value.TryGetString(out var text)
            || !node.Attributes.TryGetValue("shader", out var shaderParameter)
            || !shaderParameter.TryGetString(out var shader))
        {
            control = null;
            return false;
        }

        if (!_proto.TryIndex(shader, out ShaderPrototype? shaderProto))
        {
            control = null;
            return false;
        }

        var label = new ShaderLabel(shaderProto.InstanceUnique());
        label.Text = text;

        var font = FontTag.DefaultFont;
        var size = FontTag.DefaultSize;

        if (node.Attributes.TryGetValue("font", out var fontParameter) &&
            fontParameter.TryGetString(out var f))
            font = f;

        if (node.Attributes.TryGetValue("size", out var sizeParameter))
            size = (int) (sizeParameter.LongValue ?? size);

        if (!_proto.TryIndex<FontPrototype>(font, out var prototype))
            prototype = _proto.Index<FontPrototype>(FontTag.DefaultFont);

        var fontResource = _cache.GetResource<FontResource>(prototype.Path);

        label.FontOverride = new VectorFont(fontResource, size);

        control = label;
        return true;
    }
}
