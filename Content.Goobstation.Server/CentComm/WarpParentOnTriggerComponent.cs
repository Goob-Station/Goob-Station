namespace Content.Goobstation.Server.CentComm;

/// <summary>
/// Used to Warp the parent when triggered
/// i.e BSO lifeline
/// </summary>
[RegisterComponent]
public sealed partial class WarpParentOnTriggerComponent : Component
{
    /// <summary>
    /// Location for the warp
    /// </summary>
    [DataField]
    public string WarpLocation;
}
