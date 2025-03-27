using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are an Abductor Victim.
/// </summary>
[RegisterComponent]
public sealed partial class AbductorVictimRoleComponent : BaseMindRoleComponent;
