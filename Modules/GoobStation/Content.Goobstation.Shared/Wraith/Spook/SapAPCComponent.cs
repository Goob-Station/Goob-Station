using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Wraith.Spook;

[RegisterComponent, NetworkedComponent]
public sealed partial class SapAPCComponent : Component
{
    [DataField]
    public float SearchRange = 10f;

    [DataField]
    public float ChargeToRemove = 30000;
}
