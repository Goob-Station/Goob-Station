using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Xenobiology;

/// <summary>
/// This prototype stores information about different slime breeds.
/// </summary>
[Prototype]
public sealed partial class BreedPrototype : IPrototype
{
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
    [DataField]
    public ComponentRegistry Components = new();
}
