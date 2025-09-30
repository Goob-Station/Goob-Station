using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookScannerUiState : BoundUserInterfaceState
{
    public string Author = "";
    public string Genre = "";
    public string Title = "";
    public bool Cooldown;

    public BookScannerUiState(string author, string genre, string title, bool cooldown)
    {
        Author = author;
        Genre = genre;
        Title = title;
        Cooldown = cooldown;
    }
}
