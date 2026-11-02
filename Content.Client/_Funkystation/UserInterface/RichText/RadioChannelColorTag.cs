using Content.Client._Funkystation.Radio;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Robust.Shared.Utility;

namespace Content.Client._Funkystation.UserInterface.RichText;

/// <summary>
/// Colors text with a radio channel's color, using the player's custom channel color if they set one.
/// <c>[radiochannel="Common"]</c>, or <c>[radiochannel="Common" lang="#hex"]</c> where a valid lang color wins.
/// </summary>
public sealed class RadioChannelColorTag : IMarkupTagHandler
{
    [Dependency] private IEntitySystemManager _systems = default!;

    public string Name => "radiochannel";

    public void PushDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        if (node.Attributes.TryGetValue("lang", out var lang)
            && lang.TryGetString(out var langHex)
            && Color.TryFromHex(langHex, out var langColor))
        {
            context.Color.Push(langColor);
            return;
        }

        if (!node.Value.TryGetString(out var channelId))
        {
            context.Color.Push(ColorTag.DefaultColor);
            return;
        }

        context.Color.Push(_systems.TryGetEntitySystem<RadioChannelColorSystem>(out var colors)
            ? colors.GetColor(channelId)
            : ColorTag.DefaultColor);
    }

    public void PopDrawContext(MarkupNode node, MarkupDrawingContext context)
    {
        context.Color.Pop();
    }
}
