using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

/// <summary>
/// Contains data about positions of multiple grids
/// relatively to the center of lavaland map.
/// The main difference from <see cref="LavalandRuinPoolPrototype"/> is
/// that they are always located at same positions and don't spawn randomly.
/// </summary>
[Prototype]
public sealed partial class LavalandLayoutPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public List<LavalandLayoutEntry> Layouts = new();
}
