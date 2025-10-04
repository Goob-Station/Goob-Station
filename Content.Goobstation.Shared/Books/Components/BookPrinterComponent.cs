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

    public BookData? PrintingBook;
    public bool IsPrinting = false;
    public TimeSpan PrintEnd = TimeSpan.Zero;
    public TimeSpan NextPrint = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public enum BookPrinterUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum BookPrinterVisuals : byte
{
    Printing
}
