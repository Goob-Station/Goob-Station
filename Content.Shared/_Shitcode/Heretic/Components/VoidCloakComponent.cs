using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VoidCloakComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Transparent;
}

[Serializable, NetSerializable]
public enum VoidCloakVisuals : byte
{
    Transparent,
}
