using Content.Shared.Roles;

namespace Content.Server.Roles;

[RegisterComponent]
public sealed partial class MindcontrolledRoleComponent : AntagonistRoleComponent
{
    [DataField] public EntityUid? MasterUid = null;
}
