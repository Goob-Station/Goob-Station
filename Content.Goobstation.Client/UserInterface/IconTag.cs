using System.Diagnostics.CodeAnalysis;
using Content.Goobstation.Common.Patrons;
using Content.Shared.StatusIcon;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.UserInterface;

public sealed partial class IconTag : IMarkupTag
{
    [Dependency] private IPrototypeManager _prototype = default!;

    public string Name => "icon";

    public bool TryGetControl(MarkupNode node, [NotNullWhen(true)] out Control? control)
    {
        if (!node.Attributes.TryGetValue("src", out var id) || id.StringValue == null)
        {
            control = null;
            return false;
        }

        SpriteSpecifier? sprite = null;
        if (_prototype.TryIndex<OocIconPrototype>(id.StringValue, out var oocIcon))
            sprite = oocIcon.Icon;
        else if (_prototype.TryIndex<JobIconPrototype>(id.StringValue, out var iconPrototype))
            sprite = iconPrototype.Icon;

        var icon = new AnimatedTextureRect
        {
            SetWidth = 20,
            SetHeight = 20,
            MouseFilter = Control.MouseFilterMode.Stop,
        };
        icon.DisplayRect.Stretch = TextureRect.StretchMode.Scale;

        if (sprite != null)
            icon.SetFromSpriteSpecifier(sprite);

        if (node.Attributes.TryGetValue("tooltip", out var tooltip) && tooltip.StringValue != null)
            icon.ToolTip = tooltip.StringValue;

        control = icon;
        return true;
    }
}
