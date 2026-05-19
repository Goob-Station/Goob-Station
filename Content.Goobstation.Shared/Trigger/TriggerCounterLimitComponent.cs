using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Trigger;

/// <summary>
/// Allows the trigger to actually activate only when the
/// total amount of triggers is within a certain range.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TriggerCounterLimitComponent : Component
{
    [DataField, AutoNetworkedField]
    public int MaxCount = 1;
}
