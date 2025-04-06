namespace Content.Goobstation.Server.Condemned;

/// <summary>
/// Marks an entity as having sold their soul.
/// This means they cannot be revived, and they cannot sell their soul again.
/// </summary>
[RegisterComponent]
public sealed partial class CondemnedComponent : Component
{
    /// <summary>
    /// The entity who the component owner sold their soul to.
    /// </summary>
    [DataField]
    public EntityUid? SoulOwner;

    /// <summary>
    /// Is the entities soul owned by a corporation?
    /// </summary>
    /// <remarks>
    /// Jobs like captain, NTR, BSO, and other CC roles cannot sell their soul because Nanotrasen already owns it.
    /// </remarks>
    [DataField]
    public bool IsCorporateOwned = false;
}
