using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Roles.Components;

/// <summary>
/// Added to mind role entities to tag that they are a zombie.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GeminiRoleComponent : BaseMindRoleComponent;
