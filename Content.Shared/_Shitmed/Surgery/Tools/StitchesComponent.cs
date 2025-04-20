using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

/// <summary>
///     It lets you fucking stitch your ass up
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StitchesComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "stitches";
    public bool? Used { get; set; } = null;
    [DataField]
    public float Speed { get; set; } = 1f;
}