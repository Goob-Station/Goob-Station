namespace Content.Goobstation.Server.CentComm;

[RegisterComponent]
public sealed partial class WarpParentOnTriggerComponent : Component
{
    /// <summary>
    /// Location for the warp
    /// </summary>
    [DataField(required: true)]
    public string WarpLocation;
}
