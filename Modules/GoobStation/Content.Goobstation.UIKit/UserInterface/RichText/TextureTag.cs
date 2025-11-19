using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed class TextureTag : BaseTextureTag, IMarkupTagHandler
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Name => "tex";

    public bool TryCreateControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        control = null;

        if (!node.Attributes.TryGetValue("path", out var rawPathParameter) ||
            !rawPathParameter.TryGetString(out var rawPath))
            return false;

        if (!node.Attributes.TryGetValue("state", out var stateParameter) ||
            !stateParameter.TryGetString(out var state))
            state = "";

        if (!node.Attributes.TryGetValue("scale", out var scale) || !scale.TryGetLong(out var scaleValue))
            scaleValue = 1;

        if (!node.Attributes.TryGetValue("tooltip", out var tooltipParameter) ||
            !tooltipParameter.TryGetString(out var tooltip))
            tooltip = null;

        if (!node.Attributes.TryGetValue("offsetX", out var xParameter) ||
            !xParameter.TryGetLong(out var x))
            x = 0;

        if (!node.Attributes.TryGetValue("offsetY", out var yParameter) ||
            !yParameter.TryGetLong(out var y))
            y = 0;

        Texture tex;
        if (_prototypeManager.HasIndex<EntityPrototype>(rawPath))
        {
            var prototype = _prototypeManager.Index<EntityPrototype>(rawPath);
            tex = EntitySystemManager.GetEntitySystem<SpriteSystem>().Frame0(prototype);
        }
        else
        {
            var resPath = new ResPath(rawPath);

            SpriteSpecifier sprite =
                state == "" ? new SpriteSpecifier.Texture(resPath) : new SpriteSpecifier.Rsi(resPath, state);

            tex = EntitySystemManager.GetEntitySystem<SpriteSystem>().Frame0(sprite);
        }

        if (!TryDrawIcon(tex,
                scaleValue.Value,
                new Vector2((float) x, (float) y),
                tooltip,
                out var texture))
            return false;

        control = texture;
        return true;
    }
}
