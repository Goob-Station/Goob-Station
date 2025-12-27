using Robust.Shared.Prototypes;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[Prototype]
public sealed partial class NuclearReactorPrefabPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("parts")]
    public Dictionary<Vector2i, EntProtoId> ReactorComponents { get; private set; } = [];
}