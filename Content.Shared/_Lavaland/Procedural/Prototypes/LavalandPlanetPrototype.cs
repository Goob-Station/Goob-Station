using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// A simple wrapper that contains information about the planet, its static grid layout and a random ruin pool.
/// </summary>
[Prototype]
public sealed partial class LavalandMapPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public ProtoId<LavalandPlanetPrototype> Planet = "Lavaland";

    [DataField]
    public ProtoId<LavalandLayoutPrototype>? Layout;

    [DataField]
    public ProtoId<LavalandRuinPoolPrototype>? Ruins;
}
