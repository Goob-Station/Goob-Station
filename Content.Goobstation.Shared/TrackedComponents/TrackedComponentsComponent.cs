namespace Content.Goobstation.Shared.TrackedComponents;

[RegisterComponent]
public sealed partial class TrackedComponentsComponent : Component
{
    public Dictionary<string, HashSet<string>> ComponentsByOwner = new();

    /// <summary>
    /// these are safe for the tracker to remove once all claims are gone
    /// </summary>
    [DataField]
    public HashSet<string> TrackerCreatedComponents = [];
}
