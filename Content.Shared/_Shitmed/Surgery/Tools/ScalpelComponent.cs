using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

[RegisterComponent, NetworkedComponent]
public sealed partial class ScalpelComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "a scalpel";
    public bool? Used { get; set; } = null;
    [DataField]
    public float Speed { get; set; } = 1f;
}
