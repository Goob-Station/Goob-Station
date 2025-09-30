using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[RegisterComponent, NetworkedComponent]
public sealed partial class BookPrinterComponent : Component
{
    [DataField]
    public SoundSpecifier? PrintSound;

    [DataField]
    public bool AllowDeleting = false;

    [DataField]
    public TimeSpan NextPrint = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public enum BookPrinterUiKey : byte
{
    Key
}
