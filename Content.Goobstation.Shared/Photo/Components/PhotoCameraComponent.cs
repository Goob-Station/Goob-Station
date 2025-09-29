
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Photo;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhotoCameraComponent : Component
{
    [DataField]
    public SoundSpecifier? PhotoSound;

    [DataField]
    public SoundSpecifier? ClickSound;

    [DataField]
    public float UseDelay = 25f;

    [DataField]
    public int Uses = 5;
}

[Serializable, NetSerializable]
public enum ImageUiKey : byte
{
    Key
}
