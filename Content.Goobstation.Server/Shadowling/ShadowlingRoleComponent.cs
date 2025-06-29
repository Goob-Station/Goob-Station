using Content.Shared.Roles;

namespace Content.Goobstation.Server.Shadowling;

/// <summary>
/// Added to mind role entities to tag that they are a shadowling.
/// </summary>
[RegisterComponent]
public sealed partial class ShadowlingRoleComponent : BaseMindRoleComponent
{
    [DataField]
    public int ThrallsConverted;
}
