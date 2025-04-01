using Robust.Shared.GameStates;
using Robust.Shared.Input.Binding;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Sprinting;

[RegisterComponent, NetworkedComponent]
public sealed partial class SprinterComponent : Component
{
    [ViewVariables]
    public bool IsSprinting = false;

}



[Serializable, NetSerializable]
public sealed class SprinterComponentState : ComponentState
{
    public bool IsSprinting = false;

    public SprinterComponentState(bool isSprinting)
    {
        IsSprinting = isSprinting;
    }
}
