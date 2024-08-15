using Content.Shared.Roles;

namespace Content.Server.Roles;

[RegisterComponent]
public sealed partial class MindcontrollRoleComponent : AntagonistRoleComponent
{
     [DataField] public EntityUid? MasterUid = null;
}
