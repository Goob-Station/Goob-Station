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
    public float RuinDistance = 12;

    /// <summary>
    /// Max distance that Ruins can generate.
    /// </summary>
    [DataField]
    public float MaxDistance = 128;

    /// <summary>
    /// List of all huge ruins and their count. Should contain only really
    /// important and big ruins, that have the highest priority.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LavalandRuinPrototype>, ushort> HugeRuins = [];

    /// <summary>
    /// List of all huge ruins and their count. Contains ruins
    /// that aren't that important and can be easily skipped.
    /// </summary>
    [DataField]
    public Dictionary<ProtoId<LavalandRuinPrototype>, ushort> SmallRuins = [];
}
