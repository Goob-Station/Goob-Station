using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookPrinterUiState : BoundUserInterfaceState
{
    public Dictionary<int, BookData> Books = new();
    public bool AllowDeleting = false;
    public bool Cooldown = false;

    public BookPrinterUiState(Dictionary<int, BookData> books, bool allowDeleting, bool cooldown)
    {
        Books = books;
        AllowDeleting = allowDeleting;
        Cooldown = cooldown;
    }
}
