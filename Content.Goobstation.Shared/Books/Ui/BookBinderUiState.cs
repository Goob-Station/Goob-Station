using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class BookBinderUiState : BoundUserInterfaceState
{
    public Dictionary<NetEntity, string> Pages = new();

    public BookBinderUiState(Dictionary<NetEntity, string> pages)
    {
        Pages = pages;
    }
}
