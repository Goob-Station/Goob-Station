namespace Content.Trauma.Shared.Ranching.Components;

/// <summary>
/// Attached to ores that can be used with <see cref="PlateableChickenComponent"/>>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PlateableChickenOreComponent : Component
{
    [DataField]
    public ComponentRegistry Components;
}
