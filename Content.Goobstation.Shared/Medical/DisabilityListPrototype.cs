using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Goobstation.Shared.Medical;

/// <summary>
/// Prototype that holds a list of disability components.
/// </summary>
[Prototype]
public sealed partial class DisabilityListPrototype : IPrototype, IInheritingPrototype
{
    [ViewVariables, IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<DisabilityListPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc />
    [AbstractDataField, NeverPushInheritance]
    public bool Abstract { get; private set; }

    /// <summary>
    /// The relevant disability components.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = default!;
}
