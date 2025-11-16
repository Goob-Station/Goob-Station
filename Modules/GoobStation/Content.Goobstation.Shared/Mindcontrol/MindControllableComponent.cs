using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Mindcontrol;

/// <summary>
/// Goobstation - Component that should be used for all mobs that can be mindcontrolled (by mind control implant, revolution or enslaving)
///
/// Right now this component is fast fix for shadowling thrall. This should have it's own system with events-based checks.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindControllableComponent : Component
{
    /// <summary>
    /// True if mob is controlled by someone to prevent mind controlled guy join revolution
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public bool ControlledBySomeone = false;
}
