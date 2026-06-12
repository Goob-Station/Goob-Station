using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared.Construction.Prototypes;

/// <summary>
/// Goobstation
/// Contains construction recipes to join them together in a single group.
/// </summary>
[Prototype]
public sealed partial class ConstructionPackPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ConstructionPackPrototype>))]
    public string[]? Parents { get; private set; }

    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    [AlwaysPushInheritance]
    public HashSet<ProtoId<ConstructionPrototype>> Recipes = new();
}
