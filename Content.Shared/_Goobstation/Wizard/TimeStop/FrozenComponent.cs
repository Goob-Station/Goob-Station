using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Components;

namespace Content.Shared._Goobstation.Wizard.TimeStop;

[RegisterComponent, NetworkedComponent]
public sealed partial class FrozenComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public float FreezeTime = 10f;

    [ViewVariables(VVAccess.ReadOnly)]
    public Vector2 OldLinearVelocity = Vector2.Zero;

    [ViewVariables(VVAccess.ReadOnly)]
    public float OldAngularVelocity;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool HadCollisionWake;
}
