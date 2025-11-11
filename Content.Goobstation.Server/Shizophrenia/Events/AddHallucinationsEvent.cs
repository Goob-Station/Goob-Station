using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.Shizophrenia;

/// <summary>
/// Applies hallucinations to entity
/// </summary>
[DataDefinition]
public sealed partial class AddHallucinationsEvent : EntityEventArgs
{
    /// <summary>
    /// Hallucinations pack that will be applied
    /// </summary>
    [DataField(required: true)]
    public ProtoId<HallucinationsPackPrototype> Id;

    /// <summary>
    /// Hallucinations duration. If negative, entity will hallucinate forever
    /// </summary>
    [DataField]
    public float Duration = -1f;

    /// <summary>
    /// Whether overwrite exsisting hallucinations duration or not
    /// </summary>
    [DataField]
    public bool OverwriteTimer = false;
}
