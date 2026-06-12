using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Knowledge.Components;

/// <summary>
/// Contains knowledge entities inside with <see cref="KnowledgeComponent"/>.
/// Assigned to some physical bodies, for example brains.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KnowledgeContainerComponent : Component
{
    public const string ContainerId = "knowledge";

    /// <summary>
    /// Contains all knowledge entities.
    /// </summary>
    [ViewVariables]
    public Container? KnowledgeContainer;
}
