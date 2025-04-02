namespace Content.Goobstation.Server.DelayedTeleport;

[RegisterComponent]
public sealed partial class DelayedTeleportComponent : Component
{
    /// <summary>
    /// The UID of the map that is being teleported to.
    /// </summary>
    [DataField]
    public EntityUid MapUid;
    /// <summary>
    /// The UID of the grid that is being teleported to, if added.
    /// </summary>
    [DataField]
    public EntityUid? GridUid;
    /// <summary>
    /// The delay before the teleport occurs.
    /// </summary>
    [DataField]
    public float Delay;
    /// <summary>
    /// The time since the component was applied.
    /// </summary>
    [DataField]
    public float Elapsed;
}
