using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Trigger;

/// <summary>
/// Counts the total amount of triggers that this entity had in its entire lifetime.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerCounterComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Count;
}
