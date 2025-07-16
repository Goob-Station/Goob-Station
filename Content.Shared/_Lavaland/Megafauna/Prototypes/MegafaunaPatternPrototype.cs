using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Prototypes;

/// <summary>
/// Contains info about one action and list of
/// conditions for this action to be executed.
/// </summary>
[Prototype]
public sealed partial class MegafaunaPatternPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField(required: true)]
    public MegafaunaAction Action = default!;

    [DataField]
    public List<MegafaunaCondition> Conditions = new();
}
