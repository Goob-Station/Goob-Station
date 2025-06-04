using Content.Shared.Roles;

namespace Content.Server._Harmony.Roles;

/// <summary>
/// Added to mind role entities to tag that they are a blood brother.
/// </summary>
[RegisterComponent]
public sealed partial class BloodBrotherRoleComponent : BaseMindRoleComponent
{
    [DataField]
    public EntityUid? Brother;
}
