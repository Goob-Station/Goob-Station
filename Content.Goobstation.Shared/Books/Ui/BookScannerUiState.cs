using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookScannerUiState : BoundUserInterfaceState
{
    public string Author = "";
    public string Genre = "";
    public string Title = "";
    public string Desc = "";
    public BookScannerState State;

    public BookScannerUiState(string author, string genre, string title, string desc, BookScannerState state)
    {
        Author = author;
        Genre = genre;
        Title = title;
        Desc = desc;
        State = state;
    }
}

[Serializable, NetSerializable]
public enum BookScannerState
{
    NoBook,
    Ready,
    Scanning,
}
