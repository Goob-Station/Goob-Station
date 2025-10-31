using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge.Prototypes;

/// <summary>
/// Specifies a category of knowledge, for example is it a skill or a memory.
/// </summary>
[Prototype]
public sealed partial class KnowledgeCategoryPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;
}
