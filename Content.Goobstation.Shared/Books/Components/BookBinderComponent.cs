using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[RegisterComponent, NetworkedComponent]
public sealed partial class BookBinderComponent : Component
{
    [DataField]
    public SoundSpecifier? BookCreatedSound;

    public Container PaperContainer = default!;
}

[Serializable, NetSerializable]
public enum BookBinderUiKey : byte
{
    Key
}
