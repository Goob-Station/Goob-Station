using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicGlyphTransmuteComponent : Component
{
    /// <summary>
    ///     The search range for finding transmutation targets.
    /// </summary>
    [DataField] public float TransmuteRange = 0.5f;

    /// <summary>
    ///     A pool of entities that we pick from when transmuting.
    /// </summary>
    [DataField(required: true)]
    public HashSet<EntProtoId> Transmutations = [];

    /// <summary>
    ///     Permissible entities for the transmutation
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}
