using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class DeleteBookMessage : BoundUserInterfaceMessage
{
    public int Id;

    public DeleteBookMessage(int id)
    {
        Id = id;
    }
}
