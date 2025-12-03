using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[Serializable, NetSerializable]
public sealed class CustomBookUiState : BoundUserInterfaceState
{
    public List<string> Pages = new();

    public CustomBookUiState(List<string> pages)
    {
        Pages = pages;
    }
}
