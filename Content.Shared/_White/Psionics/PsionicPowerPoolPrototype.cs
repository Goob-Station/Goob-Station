using Robust.Shared.Prototypes;

namespace Content.Shared.Psionics;

[Prototype("psionicPowerPool")]
public sealed partial class PsionicPowerPoolPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    [ViewVariables]
    [DataField]
    public List<string> Powers = new();
}
