using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge.Prototypes;

/// <summary>
/// Prototype that specifies how hard it is to "forget" some memories or skills.
/// For example, lobotomy will remove all your job skills, but will keep all memories on their place,
/// while cloning will remove only most recent skills that still didn't settle.
/// Also controls visibility in the character menu.
/// </summary>
[Prototype]
public sealed partial class KnowledgeMemoryPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Category of that knowledge. Used for distinguishing memories from skills.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<KnowledgeCategoryPrototype> Category;

    /// <summary>
    /// Relative level of how easy it is to forget this type of knowledge.
    /// </summary>
    /// <remarks>
    /// It should've been FixedPoint2 but for some damn reason it's not available in Goobstation.Common module. AAAAAAAAAAAAAA
    /// </remarks>
    [DataField(required: true)]
    public int Level;

    /// <summary>
    /// If true, this knowledge will become permanent, unless a system removes them forcefully.
    /// Used only for debug or admin abuse.
    /// </summary>
    [DataField]
    public bool Unremoveable;

    /// <summary>
    /// If true, the knowledge won't get displayed in the Memory tab of the character menu.
    /// </summary>
    [DataField]
    public bool Hidden;

    /// <summary>
    /// Color of the sidebar in the character UI.
    /// </summary>
    [DataField]
    public Color Color = Color.White;
}
