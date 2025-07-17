using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.NTR;

/// <summary>
/// A data structure for storing historical information about bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public readonly partial record struct NtrTaskHistoryData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField]
    public string Id { get; init; } = string.Empty;

    [DataField]
    public double CompletionTime { get; init; }

    /// <summary>
    /// Whether this bounty was completed or skipped.
    /// </summary>
    [DataField]
    public TaskResult Result { get; init; } = TaskResult.Completed;

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
    [DataField]
    public ProtoId<NtrTaskPrototype> Task { get; init; }

    public NtrTaskHistoryData(NtrTaskData task, TaskResult result, TimeSpan timestamp, string? actorName)
    {
        Task = task.Task;
        Result = result;
        Id = task.Id;
        ActorName = actorName;
        Timestamp = timestamp;
        CompletionTime = timestamp.TotalSeconds;
    }

    /// <summary>
    /// Covers how a bounty was actually finished.
    /// </summary>
    public enum TaskResult
    {
        /// <summary>
        /// Bounty was actually fulfilled and the goods sold
        /// </summary>
        Completed = 0,

        /// <summary>
        /// Bounty was explicitly skipped by some actor
        /// </summary>
        Skipped = 1,

        Failed = 2,
    }
}
