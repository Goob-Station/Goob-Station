using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CorvaxGoob.ColorVisuals;

[RegisterComponent, NetworkedComponent]
public sealed partial class ColorVisualsComponent : Component;


[Serializable, NetSerializable]
public enum ColorVisuals : byte
{
    Color,
}
