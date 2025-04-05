namespace Content.Goobstation.Server.Condemned;

[RegisterComponent]
public sealed partial class CondemnedComponent : Component
{
    /// <summary>
    /// The entity who the component owner sold their soul to.
    /// </summary>
    [DataField]
    public EntityUid? SoulOwner;
}
