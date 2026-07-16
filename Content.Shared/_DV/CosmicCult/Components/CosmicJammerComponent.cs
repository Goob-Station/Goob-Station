using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's own radio jammer, because I'm not refactoring RadioJammerSystem.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicJammerComponent : Component
{
    [DataField] public float Range = 18f;
    [DataField] public bool Active = false;
}
