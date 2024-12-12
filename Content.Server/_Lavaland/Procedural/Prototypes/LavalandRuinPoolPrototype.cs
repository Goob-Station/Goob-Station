using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Prototypes;

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
    public float MaxDistance = 256;

    /// <summary>
    /// List of all ruins and their count.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LavalandRuinPrototype>, ushort> Ruins = [];
}
