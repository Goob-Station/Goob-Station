using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Knowledge.Components;

/// <summary>
/// Grants some knowledge when used in hand.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class KnowledgeGrantOnUseComponent : Component
{
    /// <summary>
    /// Knowledge to grant.
    /// </summary>
    [DataField(required:true)]
    public EntityTableSelector Table;

    /// <summary>
    /// Length of a simple doafter to learn this knowledge.
    /// </summary>
    [DataField]
    public float? DoAfter;
}
