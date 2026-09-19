using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Cyberpsychosis;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class CyberSanityThresholdEntry
{
    /// <summary>
    /// What value the effect starts at.
    /// </summary>
    [DataField]
    public float Value;

    /// <summary>
    /// Used for the cyberware UI.
    /// </summary>
    [DataField]
    public string Label = string.Empty;

    /// <summary>
    /// Used for the cyberware UI.
    /// </summary>
    [DataField]
    public string Description = string.Empty;
}

[ByRefEvent]
public readonly record struct CollectCyberSanityThresholdsEvent(List<CyberSanityThresholdEntry> Thresholds);
