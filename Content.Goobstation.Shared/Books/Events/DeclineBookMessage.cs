using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class DeclineBookMessage : EntityEventArgs
{
    public int Book;

    public DeclineBookMessage(int book)
    {
        Book = book;
    }
}
