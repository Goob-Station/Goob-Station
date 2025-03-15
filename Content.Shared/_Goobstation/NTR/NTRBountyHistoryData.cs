using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.NTR;

/// <summary>
/// A data structure for storing historical information about bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct NTRBountyHistoryData
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
    public BountyResult Result { get; init; } = BountyResult.Completed;

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
    public ProtoId<NTRBountyPrototype> Bounty { get; init; } = string.Empty;

    public NTRBountyHistoryData(NTRBountyData bounty, BountyResult result, TimeSpan timestamp, string? actorName)
    {
        Bounty = bounty.Bounty;
        Result = result;
        Id = bounty.Id;
        ActorName = actorName;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Covers how a bounty was actually finished.
    /// </summary>
    public enum BountyResult
    {
        /// <summary>
        /// Bounty was actually fulfilled
        /// </summary>
        Completed = 0,

        /// <summary>
        /// Bounty was explicitly skipped by some actor
        /// </summary>
        Skipped = 1,
    }
}
