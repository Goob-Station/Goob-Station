using Content.Client._Funkystation.UserInterface.RichText;
using Robust.Client.UserInterface.RichText;

namespace Content.Client.Examine;

public sealed partial class ExamineSystem
{
    private static readonly Type[] ExamineTooltipTags =
    [
        typeof(BoldItalicTag),
        typeof(BoldTag),
        typeof(BulletTag),
        typeof(ColorTag),
        typeof(HeadingTag),
        typeof(ItalicTag),
        typeof(RadioChannelColorTag),
    ];
}
