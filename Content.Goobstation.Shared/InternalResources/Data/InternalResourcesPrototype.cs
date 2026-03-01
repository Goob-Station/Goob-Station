using Content.Shared.Alert;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.Data;

/// <summary>
/// Prototype for internal resources type. Mostly contain visualization and information data.
/// </summary>
[Prototype]
public sealed class InternalResourcesPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public LocId Name;

    [DataField]
    public LocId? Description;

    /// <summary>
    /// The alert prototype to be shown.
    /// </summary>
    [DataField("alert", required: true)]
    public ProtoId<AlertPrototype> AlertPrototype;

    /// <summary>
    /// The thresholds proto used for raising InternalResourcesThresholdMetEvent.
    /// </summary>
    [DataField("thresholds")]
    public ProtoId<InternalResourcesThresholdsPrototype>? ThresholdsProto;

    /// <summary>
    /// Base resource regeneration amount per update tick
    /// </summary>
    [DataField("regenerationRate")]
    public float BaseRegenerationRate = 1f;

    /// <summary>
    /// Base resource maximum amount
    /// </summary>
    [DataField("maxAmount")]
    public float BaseMaxAmount = 100f;

    /// <summary>
    /// Starting resource amount when added to an entity.
    /// </summary>
    [DataField("startingAmount")]
    public float BaseStartingAmount = 100f;

    /// <summary>
    /// Used for action popups when the resource amount is not high enough.
    /// </summary>
    [DataField]
    public LocId DeficitPopup = "internal-resources-action-generic-deficit";
}
