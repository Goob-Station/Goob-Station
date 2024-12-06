using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Procedural.Prototypes;

[Prototype]
public sealed partial class LavalandRuinPoolPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField]
    public List<ProtoId<LavalandRuinPrototype>> Ruins = [];
}
