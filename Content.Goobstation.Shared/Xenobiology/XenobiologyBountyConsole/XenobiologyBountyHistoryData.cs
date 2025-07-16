using Content.Shared.Cargo;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Xenobiology.XenobiologyBountyConsole;

/// <summary>
/// A data structure for storing historical information about bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct XenobiologyBountyHistoryData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Whether this bounty was completed or skipped.
    /// </summary>
    [DataField]
    public CargoBountyHistoryData.BountyResult Result { get; init; } = CargoBountyHistoryData.BountyResult.Completed;

    /// <summary>
    /// Optional name of the actor that completed/skipped the bounty.
    /// </summary>
    [DataField]
    public string? ActorName { get; init; } = default;

    /// <summary>
    /// Time when this bounty was completed or skipped
    /// </summary>
    [DataField]
    public TimeSpan Timestamp { get; init; } = TimeSpan.MinValue;

    /// <summary>
    /// The prototype containing information about the bounty.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<XenobiologyBountyPrototype> Bounty { get; init; } = string.Empty;

    public XenobiologyBountyHistoryData(XenobiologyBountyData bounty, CargoBountyHistoryData.BountyResult result, TimeSpan timestamp, string? actorName)
    {
        Bounty = bounty.Bounty;
        Result = result;
        Id = bounty.Id;
        ActorName = actorName;
        Timestamp = timestamp;
    }
}
