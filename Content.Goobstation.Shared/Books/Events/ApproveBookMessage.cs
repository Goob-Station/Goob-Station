using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Approve pending book, sent from client to server
/// </summary>
[Serializable, NetSerializable]
public sealed class ApproveBookMessage : EntityEventArgs
{
    public int Book;

    public ApproveBookMessage(int book)
    {
        Book = book;
    }
}
