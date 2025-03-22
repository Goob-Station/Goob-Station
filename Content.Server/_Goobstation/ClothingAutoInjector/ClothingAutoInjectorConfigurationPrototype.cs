using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.ClothingAutoInjector;

[DataDefinition]
[Prototype("clothingAutoInjectorConfiguration")]
public sealed partial class ClothingAutoInjectorConfigurationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; }

    /// <summary>
    /// Which chems, and how much?
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();
}
