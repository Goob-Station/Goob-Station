using Content.Shared.Roles;

namespace Content.Server.Roles;

[RegisterComponent]
public sealed partial class MindcontrolledRoleComponent : BaseMindRoleComponent
{
    [DataField] public EntityUid? MasterUid = null;
}
