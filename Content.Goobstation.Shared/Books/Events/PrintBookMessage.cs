using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class PrintBookMessage : BoundUserInterfaceMessage
{
    public BookData Book;

    public PrintBookMessage(BookData book)
    {
        Book = book;
    }
}
