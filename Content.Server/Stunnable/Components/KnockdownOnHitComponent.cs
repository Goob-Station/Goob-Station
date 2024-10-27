using Content.Shared.Standing;

namespace Content.Server.Stunnable.Components;

[RegisterComponent]
public sealed partial class KnockdownOnHitComponent : Component
{
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField]
    public DropHeldItemsBehavior DropHeldItemsBehavior = DropHeldItemsBehavior.NoDrop;

    [DataField]
    public bool RefreshDuration = true;
}
