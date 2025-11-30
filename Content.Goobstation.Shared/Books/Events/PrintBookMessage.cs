using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Sent from printer ui to server to actually print the book
/// </summary>
[Serializable, NetSerializable]
public sealed class PrintBookMessage : BoundUserInterfaceMessage
{
    public BookData Book;

    public PrintBookMessage(BookData book)
    {
        Book = book;
    }
}
