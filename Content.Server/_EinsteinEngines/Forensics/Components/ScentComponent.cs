namespace Content.Server._EinsteinEngines.Forensics.Components;

/// <summary>
/// This component is for mobs that have a Scent.
/// </summary>
[RegisterComponent]
public sealed partial class ScentComponent : Component
{
    [DataField]
    public string Scent = string.Empty;
}
