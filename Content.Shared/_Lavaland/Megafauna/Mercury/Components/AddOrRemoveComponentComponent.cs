using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Mercury.Components;

/// <summary>
/// Adds or removes a specified component. Additionally, can add or remove it on a timer.
/// Empty component cause variables go in the event. Why woudld you add a component to a YAML entity to remove another component lol
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AddOrRemoveComponentComponent : Component
{
    public ComponentRegistry? TargetComponent;

    [AutoNetworkedField]
    public bool RemoveAfterTimer;

    [AutoNetworkedField]
    public TimeSpan TimeToRemoval;

    public TimeSpan Accumulator;
}
