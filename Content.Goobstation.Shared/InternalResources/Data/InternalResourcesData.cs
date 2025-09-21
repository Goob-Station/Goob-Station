using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.InternalResources.Data;

/// <summary>
/// Data structure for storing and changing inner resource in entities
/// </summary>
[Serializable, NetSerializable]
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

    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<InternalResourcesPrototype>))]
    public string? InternalResourcesType;

    public InternalResourcesData(float maxAmount, float regenerationRate, float startingAmount)
    {
        CurrentAmount = startingAmount;
        MaxAmount = maxAmount;
        RegenerationRate = regenerationRate;
    }
}
