using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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
    [DataField(required: true)]
    public List<EntProtoId> ToAdd = new();

    /// <summary>
    /// Length of a simple doafter to learn this knowledge.
    /// </summary>
    [DataField]
    public float? DoAfter;
}
