using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Hastur.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class HasturDevouringComponent : Component;

[NetSerializable, Serializable]
public enum DevourVisuals : byte
{
    Devouring,
}

