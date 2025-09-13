namespace Content.Goobstation.Server.StationEvents.Components;

/// <summary>
/// this is a component for SubstationFaultRule
/// </summary>
[RegisterComponent]
public sealed partial class SubstationCanFailComponent : Component
{
    [DataField]
    public bool CanBeDeactivated = true;
}
