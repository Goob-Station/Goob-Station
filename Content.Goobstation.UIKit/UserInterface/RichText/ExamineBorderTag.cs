using Robust.Client.UserInterface.RichText;

namespace Content.Goobstation.UIKit.UserInterface.RichText;

public sealed partial class ExamineBorderTag : IMarkupTagHandler
{
    [Dependency] private IEntitySystemManager _entitySystemManager = default!;

    public const string TagName = "examineborder";

    public string Name => TagName;
}
