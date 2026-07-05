using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Pirate.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a blood brother.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodBrotherRoleComponent : BaseMindRoleComponent
{
    [DataField, AutoNetworkedField]
    public EntityUid? Brother;
}
