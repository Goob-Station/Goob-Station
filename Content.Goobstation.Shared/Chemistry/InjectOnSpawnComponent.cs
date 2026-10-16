using Content.Shared.Chemistry.Reagent;

namespace Content.Goobstation.Shared.Chemistry;

/// <summary>
/// Inject entity with this component with some reagent on spawn
/// </summary>
[RegisterComponent]
public sealed partial class InjectOnSpawnComponent : Component
{
    /// <summary>
    /// Reagent injected with
    /// </summary>
    [DataField(required:true)]
    public List<ReagentQuantity> Reagents;

    [DataField(required: true)]
    public string? SolutionName;
}
