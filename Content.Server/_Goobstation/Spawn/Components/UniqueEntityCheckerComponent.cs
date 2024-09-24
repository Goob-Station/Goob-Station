namespace Content.Server.Goobstation.Spawn.Components;

/// <summary>
///     Ensures that related entity will be on station (like NTR or BSO lockers) and will be not duplicate.
///     If station have unique entity - item with this component will be deleted.
/// </summary>
[RegisterComponent]
public sealed partial class UniqueEntityCheckerComponent : Component
{
    /// <summary>
    ///     Name of marker in UniqueEntityMarker
    /// </summary>
    [DataField]
    public string? MarkerName;

    /// <summary>
    ///     If true - it will check entities with StationDataComponent
    ///     If false - it will check entities globally
    /// </summary>
    [DataField]
    public bool StationOnly = true;
}
