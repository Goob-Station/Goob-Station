namespace Content.Shared._Lavaland.Elevator;

/// <summary>
/// Marks beacon to teleport to.
/// </summary>
[RegisterComponent]
public sealed partial class ElevatorBeaconComponent : Component
{
    [DataField(required: true)]
    public string Id = string.Empty;
}
