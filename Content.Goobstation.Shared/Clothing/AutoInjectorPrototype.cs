using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Clothing;

/// <summary>
/// Prototype for clothing-integrated autoinjectors. Could be used for modsuits in the future.
/// </summary>
[Prototype("autoInjector")]
public sealed class AutoInjectorPrototype : IPrototype
{
    /// <summary>
    /// The unique identifier for the prototype.
    /// </summary>
    [IdDataField]
    public string ID { get; private init; } = default!;

    /// <summary>
    /// Dictionary of reagents and their quantities to be injected.
    /// Key: Reagent ID, Value: Quantity to inject.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FixedPoint2> Reagents = new();

}
