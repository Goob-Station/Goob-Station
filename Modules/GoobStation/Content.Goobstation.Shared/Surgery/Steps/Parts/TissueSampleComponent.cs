using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Surgery.Steps.Parts;

/// <summary>
/// Component for xeno tissue sample, used in the graft issue surgery step.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TissueSampleComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "a xeno tissue sample";

    [DataField]
    public bool? Used { get; set; } = true;

    [DataField]
    public float Speed { get; set; } = 1f;
}
