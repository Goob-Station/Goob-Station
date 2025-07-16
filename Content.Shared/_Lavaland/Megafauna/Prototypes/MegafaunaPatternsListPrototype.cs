using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Prototypes;

/// <summary>
/// Contains a list of patterns that one megafauna can execute.
/// You can also specify priority for all attacks to eliminate cases
/// when multiple attacks are available from the list.
/// </summary>
[Prototype]
public sealed partial class MegafaunaPatternsListPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public List<ProtoId<MegafaunaPatternPrototype>> Entries = new();
}
