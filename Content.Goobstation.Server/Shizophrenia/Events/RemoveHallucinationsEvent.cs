using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

/// <summary>
/// Removes hallucinations with specified key from entity
/// </summary>
[DataDefinition]
public sealed partial class RemoveHallucinationsEvent : EntityEventArgs
{
    /// <summary>
    /// Hallucinations pack to remove
    /// </summary>
    [DataField]
    public ProtoId<HallucinationsPackPrototype> Id = "";
}
