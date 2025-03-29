using Content.Goobstation.Common.Standing;

namespace Content.Goobstation.Shared.Knockdown;

[RegisterComponent]
public sealed partial class KnockdownOnCollideComponent : Component
{
    [DataField]
    public DropHeldItemsBehavior Behavior = DropHeldItemsBehavior.NoDrop;
}
