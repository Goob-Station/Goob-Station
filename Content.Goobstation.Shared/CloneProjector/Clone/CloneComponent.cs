namespace Content.Goobstation.Shared.CloneProjector.Clone;

[RegisterComponent]
public sealed partial class CloneComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? HostProjector;

    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? HostEntity;

}
