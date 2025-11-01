using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookData : IEquatable<BookData>
{
    public string Title = "";
    public string Genre = "";
    public string Author = "";
    public string Desc = "";
    public List<string> Pages = new();
    public Dictionary<string, (ResPath Path, string State)> Binding = new();

    public BookData(string title, string genre, string author, string desc, List<string> pages, Dictionary<string, (ResPath Path, string State)> binding)
    {
        Title = title;
        Genre = genre;
        Author = author;
        Desc = desc;
        Pages = pages;
        Binding = binding;
    }

    public bool Equals(BookData? other)
    {
        if (other == null)
            return false;

        return Title == other.Title && Genre == other.Genre && Author == other.Author && Desc == other.Desc && Pages.Equals(other.Pages) && Binding.Equals(other.Binding);
    }
}
