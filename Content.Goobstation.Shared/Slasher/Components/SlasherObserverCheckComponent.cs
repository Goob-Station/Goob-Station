using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Slasher.Components;

/// <summary>
/// Check for observers.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlasherObserverCheckComponent : Component
{
    /// <summary>
    /// Whether the slasher is currently being observed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsObserved;

    /// <summary>
    /// Range to check for observers with line of sight.
    /// </summary>
    [DataField]
    public float Range = 10f;

    /// <summary>
    /// The alert prototype to show.
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> Alert = "SlasherSeen";
}
