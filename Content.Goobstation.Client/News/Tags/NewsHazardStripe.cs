using System.Diagnostics.CodeAnalysis;
using Content.Client.Guidebook.Richtext;
using Content.Goobstation.UIKit.UserInterface.Lobby;
using Robust.Client.UserInterface;

namespace Content.Goobstation.Client.News.Tags;

public sealed class NewsHazardStripe : IDocumentTag
{
    public bool TryParseTag(Dictionary<string, string> args, [NotNullWhen(true)] out Control? control)
    {
        var stripe = new HazardStripe
        {
            HorizontalAlignment = NewsTagArgs.HAlign(args, Control.HAlignment.Stretch),
            Margin = NewsTagArgs.Margin(args),
        };

        if (args.TryGetValue("Height", out var height))
            stripe.MinHeight = NewsTagArgs.Float(height, 6f);

        if (args.TryGetValue("Width", out var width))
            stripe.MinWidth = NewsTagArgs.Float(width, 0f);

        control = stripe;
        return true;
    }
}
