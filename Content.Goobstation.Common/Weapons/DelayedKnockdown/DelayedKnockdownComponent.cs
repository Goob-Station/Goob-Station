using Robust.Shared.GameObjects;
using Robust.Shared.ViewVariables;

namespace Content.Goobstation.Common.Weapons.DelayedKnockdown;

[RegisterComponent]
public sealed partial class DelayedKnockdownComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float Time = float.MaxValue;

    [ViewVariables(VVAccess.ReadWrite)]
    public float KnockdownTime = 0f;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Refresh = true;
}
