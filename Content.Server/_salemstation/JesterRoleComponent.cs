using Content.Shared.Roles;

namespace Content.Server._salemstation;

[RegisterComponent]
public sealed partial class JesterRoleComponent : BaseMindRoleComponent
{
    public bool ExecutedByTribunal;
    public bool GhostKillUsed;
    public EntityUid? GhostKillAction;
}
