
namespace Content.Goobstation.Server.CentComm;

/// <summary>
/// Used for CentComm lifeline teleport
/// </summary>
[RegisterComponent]
public sealed partial class BSOLifelineComponent : Component
{
    /// <summary>
    /// Location for the warp
    /// </summary>
    [DataField]
    public string WarpLocation { get; set; } = "CentComm";
}
