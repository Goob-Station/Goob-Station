using Content.Goobstation.Shared.CloneProjector;

namespace Content.Goobstation.Server.CloneProjector;

[RegisterComponent]
public sealed partial class WearingCloneProjectorComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Entity<CloneProjectorComponent>? ConnectedProjector;
}
