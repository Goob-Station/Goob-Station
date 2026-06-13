using Robust.Shared.GameStates;

namespace Content.Shared.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a syndicate traitor.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TraitorRoleComponent : BaseMindRoleComponent
{
    /// <summary>
    ///     Goobstation - traitor flavor.
    /// </summary>
    [DataField] public string ObjectiveIssuer = string.Empty;
}

