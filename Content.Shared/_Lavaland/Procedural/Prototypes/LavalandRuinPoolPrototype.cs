using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Procedural.Prototypes;

[Prototype]
public sealed partial class LavalandRuinPoolPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    /// <summary>
    /// Distance in-between ruins.
    /// </summary>
    [DataField]
    public float RuinDistance = 24;

    /// <summary>
    /// Max distance that Ruins can generate.
    /// </summary>
    [DataField]
    public float MaxDistance = 336;

    /// <summary>
    /// List of all grid ruins and their count.
    /// Used for ruins that are loaded as proper grids.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LavalandGridRuinPrototype>, ushort> GridRuins = new();

    /// <summary>
    /// List of all dungeon ruins and their count.
    /// Used for ruins that are generated with Dungeon markers.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LavalandDungeonRuinPrototype>, ushort> DungeonRuins = new();
}
