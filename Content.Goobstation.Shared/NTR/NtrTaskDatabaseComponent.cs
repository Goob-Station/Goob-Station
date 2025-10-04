using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Goobstation.Shared.NTR;

/// <summary>
/// Stores all active cargo bounties for a particular station.
/// </summary>
[RegisterComponent]
public sealed partial class NtrTaskDatabaseComponent : Component
{
    /// <summary>
    /// Maximum amount of bounties a station can have.
    /// </summary>
    [DataField]
    public int MaxTasks = 5;

    /// <summary>
    /// A list of all the bounties currently active for a station.
    /// </summary>
    [DataField]
    public List<NtrTaskData> Tasks = new();

    /// <summary>
    /// A list of all the bounties that have been completed or
    /// skipped for a station.
    /// </summary>
    [DataField]
    public List<NtrTaskHistoryData> History = new();

    /// <summary>
    /// Used to determine unique order IDs
    /// </summary>
    [DataField]
    public int TotalTasks;

    /// <summary>
    /// A list of bounty IDs that have been checked this tick.
    /// Used to prevent multiplying bounty prices.
    /// </summary>
    [DataField]
    public HashSet<string> CheckedTasks = new();

    /// <summary>
    /// The time at which players will be able to skip the next bounty.
    /// </summary>
    [DataField]
    public TimeSpan NextSkipTime = TimeSpan.Zero;

    /// <summary>
    /// The time between skipping bounties.
    /// </summary>
    [DataField]
    public TimeSpan SkipDelay = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Time between automatic task generation
    /// </summary>
    [DataField]
    public TimeSpan TaskGenerationDelay = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Time when next task will be generated
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextTaskGenerationTime = TimeSpan.Zero;

    /// <summary>
    /// Tracks which task prototypes have been printed
    /// </summary>
    [DataField]
    public HashSet<string> PrintedPrototypes = new();

    [DataField] // if u cant complete a task in 20 mins, skill issue.
    public TimeSpan MaxActiveTime = TimeSpan.FromMinutes(20); // 20 min to complete a task
}
