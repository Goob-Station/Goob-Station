using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.MobPhases;

[Prototype]
public sealed partial class MobPhaseComponentsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public ComponentRegistry Components = new();
}
