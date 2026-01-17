using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

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

    [DataField(required: true)]
    public LocId Name;

    [DataField(required: true)]
    public LocId Description;

    /// <summary>
    /// Color of the sidebar in the character UI.
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    /// <summary>
    /// Sprite to display in the character menu.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Sprite;
}
