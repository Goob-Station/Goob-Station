using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Decline pending book, sent from client to server
/// </summary>
[Serializable, NetSerializable]
public sealed class DeclineBookMessage : EntityEventArgs
{
    public int Book;

    public DeclineBookMessage(int book)
    {
        Book = book;
    }
}
