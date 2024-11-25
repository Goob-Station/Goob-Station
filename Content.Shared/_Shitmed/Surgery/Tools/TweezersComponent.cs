using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Medical.Surgery.Tools;

/// <summary>
///     Like Hemostat but lets ghetto tools be used differently for clamping and removing organs.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TweezersComponent : Component, ISurgeryToolComponent
{
    public string ToolName => "tweezers";
    public bool? Used { get; set; } = null;
    [DataField]
    public float Speed { get; set; } = 1f;
}
