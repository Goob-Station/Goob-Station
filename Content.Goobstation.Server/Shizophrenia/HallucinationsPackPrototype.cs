using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

[Prototype]
public sealed partial class HallucinationsPackPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public BaseHallucinationsType Data = default!;
}
