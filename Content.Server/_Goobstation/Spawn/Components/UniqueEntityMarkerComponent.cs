namespace Content.Server.Goobstation.Spawn.Components;

/// <summary>
///     Component-marker for unique entity
/// </summary>
[RegisterComponent]
public sealed partial class UniqueEntityMarkerComponent : Component
{
    /// <summary>
    ///     Marker name that would be used in check
    /// </summary>
    [DataField]
    public string? MarkerName;
}
