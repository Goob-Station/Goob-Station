using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.InternalResources.Data;

/// <summary>
/// Data structure for storing and changing inner resource in entities
/// </summary>
[Serializable, NetSerializable]
[DataDefinition]
public sealed partial class InternalResourcesData
{
    /// <summary>
    /// Current amount of resources
    /// </summary>
    [DataField]
    public float CurrentAmount = 0;

    /// <summary>
    /// Maximum amount of resources
    /// </summary>
    [DataField]
    public float MaxAmount = 100;

    /// <summary>
    /// Resources regeneration rate per update time
    /// </summary>
    [DataField]
    public float RegenerationRate = 1f;

    /// <summary>
    /// The thresholds at which InternalResourcesThresholdMetEvent will be fired.
    /// </summary>
    [DataField]
    public Dictionary<InternalResourcesThreshold, (float, bool)>? Thresholds = new();

    /// <summary>
    /// Prototype with visual information of internal resources
    /// </summary>
    [DataField(required: true)]
    public ProtoId<InternalResourcesPrototype> InternalResourcesType;

    public InternalResourcesData(
        float maxAmount,
        float regenerationRate,
        float startingAmount,
        Dictionary<InternalResourcesThreshold, (float, bool)>? thresholds,
        string protoId)
    {
        CurrentAmount = startingAmount;
        MaxAmount = maxAmount;
        RegenerationRate = regenerationRate;
        Thresholds = thresholds;
        InternalResourcesType = protoId;
    }
}

// this feels kinda shit but it'll do for now
// add more thresholds when needed.
public enum InternalResourcesThreshold : byte
{
    Threshold1,
    Threshold2,
    Threshold3,
    Threshold4,
}
