using Content.Goobstation.Common.Knowledge.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Goobstation.Common.Knowledge.Components;

/// <summary>
/// Stores information about a set of knowledge units, assigned
/// to a dummy entity that is parented to some entity with <see cref="KnowledgeContainerComponent"/>, usually a brain.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class KnowledgeComponent : Component
{
    /// <summary>
    /// The entity that this knowledge is applied to. (Entity with <see cref="KnowledgeContainerComponent"/>)
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? AppliedTo;

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

    /// <summary>
    /// Sprite to display in the character UI.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Sprite;
}
