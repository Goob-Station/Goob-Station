namespace Content.Server._Goobstation.Spawn.Components;

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

    /// <summary>
    ///     If true - marker will work on grids with StationDataComponent
    ///     If false - marker will work globally
    /// </summary>
    [DataField]
    public bool StationOnly = true;
}
