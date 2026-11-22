using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Goobstation.Shared.Xenobiology;

/// <summary>
/// This prototype stores information about different slime breeds.
/// </summary>
[Prototype]
public sealed partial class BreedPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<BreedPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    [IdDataField]
    public string ID { get; private set; } = null!;

    /// <summary>
    /// Used to set the slime's name.
    /// </summary>
    [DataField(required: true)]
    public string BreedName = string.Empty;

    /// <summary>
    /// The extract produced when this breed is ground.
    /// </summary>
    [DataField]
    public EntProtoId ProducedExtract = "GreySlimeExtract";

    /// <summary>
    /// What components should be given to the slime mob? Usually SlimeComponent.
    /// </summary>
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry Components = new();
}
