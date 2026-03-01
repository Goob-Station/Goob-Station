namespace Content.Goobstation.Server.SpaceWhale;

/// <summary>
/// Marks an entity for a space whale target.
/// </summary>
[RegisterComponent]
public sealed partial class SpaceWhaleTargetComponent : Component
{
    [DataField] public EntityUid Entity { get; set; }
}
