using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Nullspace;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NullspaceClownComponent : Component
{
    [DataField, AutoNetworkedField]
    public NullspaceClownState State = NullspaceClownState.Hunt;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;
}

[Serializable, NetSerializable]
public enum NullspaceClownState : byte
{
    Hunt,
    Search,
    Chase,
    Caught,
    Peek,
}
