using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookScannerUiState : BoundUserInterfaceState
{
    public string Author = "";
    public string Genre = "";
    public string Title = "";
    public string Desc = "";
    public bool Cooldown;

    public BookScannerUiState(string author, string genre, string title, string desc, bool cooldown)
    {
        Author = author;
        Genre = genre;
        Title = title;
        Desc = desc;
        Cooldown = cooldown;
    }
}
