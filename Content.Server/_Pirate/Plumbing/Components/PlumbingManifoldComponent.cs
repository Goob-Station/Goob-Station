namespace Content.Server._Pirate.Plumbing.Components;

[RegisterComponent]
public sealed partial class PlumbingManifoldComponent : Component
{
    /// <summary>
    /// Node names that belong to manifold side A.
    /// </summary>
    [DataField("sideA")]
    public HashSet<string> SideANodeNames { get; set; } = new();

    /// <summary>
    /// Node names that belong to manifold side B.
    /// </summary>
    [DataField("sideB")]
    public HashSet<string> SideBNodeNames { get; set; } = new();
}
