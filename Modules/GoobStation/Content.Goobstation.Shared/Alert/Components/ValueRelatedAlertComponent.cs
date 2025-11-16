using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Alert.Components;

/// <summary>
/// Generic component for alerts that have needs to update when some value in some component changes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ValueRelatedAlertComponent : Component
{
    [DataField]
    public short MaxSeverity = 0;

    [DataField]
    public string IconPrefix = "";
}
