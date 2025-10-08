using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Sent to all admins when new pending book is added or exsisting is removed
/// </summary>
[Serializable, NetSerializable]
public sealed class PopulatePendingBooksMenuMessage : EntityEventArgs
{
    public Dictionary<int, BookData> Books = new();

    public PopulatePendingBooksMenuMessage(Dictionary<int, BookData> books)
    {
        Books = books;
    }
}
