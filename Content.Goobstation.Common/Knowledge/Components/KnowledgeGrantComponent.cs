using Robust.Shared.Prototypes;

namespace Content.Goobstation.Common.Knowledge.Components;

/// <summary>
/// Grants knowledge to the entity automatically on mapinit, then removes itself.
/// </summary>
[RegisterComponent]
public sealed partial class KnowledgeGrantComponent : Component
{
    /// <summary>
    /// Knowledge that will be added.
    /// </summary>
    [DataField(required:true)]
    public List<EntProtoId> ToAdd = new();
}
