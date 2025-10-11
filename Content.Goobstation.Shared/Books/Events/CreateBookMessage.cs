using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class CreateBookMessage : BoundUserInterfaceMessage
{
    public string Title = "";
    public string Genre = "";
    public string AuthorName = "";
    public List<string> Pages = new();
    public string Description = "";
    public Dictionary<string, (ResPath Path, string State)> Binding = new();

    public CreateBookMessage(string title, string genre, string authorName, List<string> pages, string desc, Dictionary<string, (ResPath Path, string State)> binding)
    {
        Title = title;
        Genre = genre;
        AuthorName = authorName;
        Pages = pages;
        Description = desc;
        Binding = binding;
    }
}
