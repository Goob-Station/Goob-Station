using Robust.Shared.GameStates;

namespace Content.Shared.Body.Organ;

[RegisterComponent, NetworkedComponent]

/// <summary>
/// This marks any organ with the component as being unremovable through surgery.
/// </summary>
public sealed partial class UnremovableOrganComponent : Component;
