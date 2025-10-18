using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

/// <summary>
/// Sent from admin printer ui to server to remove book FROM DB
/// </summary>
[Serializable, NetSerializable]
public sealed class DeleteBookMessage : BoundUserInterfaceMessage
{
    public int Id;

    public DeleteBookMessage(int id)
    {
        Id = id;
    }
}
