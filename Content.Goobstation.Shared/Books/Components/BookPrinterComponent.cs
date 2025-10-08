using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[RegisterComponent, NetworkedComponent]
public sealed partial class BookPrinterComponent : Component
{
    [DataField]
    public SoundSpecifier? PrintSound;

    /// <summary>
    /// Whether book deleting is allowed
    /// </summary>
    [DataField]
    public bool AllowDeleting = false;

    // printing data down
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
