using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Clothing;

[Prototype("autoInjector")]
public sealed class AutoInjectorPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private init; } = default!;

    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();

}
