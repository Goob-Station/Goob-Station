using Robust.Shared.GameStates;

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
}
