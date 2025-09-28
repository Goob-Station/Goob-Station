
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Photo;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhotoCameraComponent : Component
{

}

[Serializable, NetSerializable]
public enum ImageUiKey : byte
{
    Key
}
