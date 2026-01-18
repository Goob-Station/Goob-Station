using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Prototypes;

/// <summary>
/// Goobstation
/// Tag prototype for splitting all constructions into groups.
/// </summary>
[Prototype]
public sealed partial class ConstructionGroupPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public LocId Name;
}
