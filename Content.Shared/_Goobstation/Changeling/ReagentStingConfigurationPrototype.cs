using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[DataDefinition]
[Prototype("reagentStingConfiguration")]
public sealed partial class ReagentStingConfigurationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();
}
