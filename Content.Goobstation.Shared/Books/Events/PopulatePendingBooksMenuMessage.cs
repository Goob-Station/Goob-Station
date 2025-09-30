using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class PopulatePendingBooksMenuMessage : EntityEventArgs
{
    public Dictionary<int, BookData> Books = new();

    public PopulatePendingBooksMenuMessage(Dictionary<int, BookData> books)
    {
        Books = books;
    }
}
