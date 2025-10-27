using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult;

/// <summary>
/// When a <c>MonumentOnDespawnComponent</c> despawns, a monument will spawn in its place with the same cult association
/// </summary>
[RegisterComponent, Access(typeof(SharedMonumentSystem))]
public sealed partial class MonumentOnDespawnComponent : Component
{
    /// <summary>
    /// Entity prototype to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype = string.Empty;
}
