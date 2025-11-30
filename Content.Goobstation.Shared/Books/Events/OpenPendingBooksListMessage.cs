using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Opens pending books list
/// </summary>
[Serializable, NetSerializable]
public sealed class OpenPendingBooksListMessage : EntityEventArgs
{
    public Dictionary<int, BookData> Books = new();

    public OpenPendingBooksListMessage(Dictionary<int, BookData> books)
    {
        Books = books;
    }
}
