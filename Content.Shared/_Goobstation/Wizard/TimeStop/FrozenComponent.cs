using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.TimeStop;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrozenComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float FreezeTime = 10f;

    [ViewVariables(VVAccess.ReadWrite)]
    public Vector2 OldLinearVelocity = Vector2.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public float OldAngularVelocity;
}
