namespace Content.Goobstation.Shared.Maps;

/// <summary>
/// Component for Hell Map
/// </summary>
[RegisterComponent]
public sealed partial class HellMapComponent : Component
{
    [DataField] public EntityUid? ExitPortal;
}
