using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Books;

[RegisterComponent, NetworkedComponent]
public sealed partial class BookScannerComponent : Component
{
    [DataField]
    public SoundSpecifier? ScanSound;

    public EntityUid? Book;

    public bool IsScanning = false;

    public TimeSpan ScanEndTime = TimeSpan.Zero;
}

[Serializable, NetSerializable]
public enum BookScannerUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum BookScannerVisuals : byte
{
    Scanning
}
