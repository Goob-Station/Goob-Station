using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.NTR;

[RegisterComponent, NetworkedComponent]
public sealed partial class NtrTaskProviderComponent : Component
{
    /// <summary>
    /// The sound made when the bounty is skipped.
    /// </summary>
    [DataField("skipSound")]
    public SoundSpecifier SkipSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound made when bounty skipping is denied due to lacking access.
    /// </summary>
    [DataField("denySound")]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");

    /// <summary>
    /// The time at which the console will be able to print a label again.
    /// </summary>
    [DataField("nextPrintTime", customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField("taskLabelId", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string TaskLabelId = "Paper";

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField("printDelay")]
    public TimeSpan PrintDelay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The sound made when printing occurs
    /// </summary>
    [DataField("printSound")]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    [DataField]
    public HashSet<string> ActiveTaskIds = new();
}

[NetSerializable, Serializable]
public sealed class NtrTaskProviderState : BoundUserInterfaceState
{
    public List<NtrTaskData> AvailableTasks { get; }
    public List<NtrTaskHistoryData> History { get; }
    public TimeSpan UntilNextSkip { get; }
    public HashSet<string> LockedTasks { get; }

    public NtrTaskProviderState(
        List<NtrTaskData> available,
        List<NtrTaskHistoryData> history,
        TimeSpan skipTime,
        HashSet<string>? locked = null)
    {
        AvailableTasks = available;
        History = history;
        UntilNextSkip = skipTime;
        LockedTasks = locked ?? new HashSet<string>();
    }
}

[Serializable, NetSerializable]
public sealed class TaskSkipMessage : BoundUserInterfaceMessage
{
    public string TaskId;

    public TaskSkipMessage(string taskId)
    {
        TaskId = taskId;
    }
}
[Serializable, NetSerializable]
public sealed class TaskPrintLabelMessage : BoundUserInterfaceMessage
{
    public string TaskId;

    public TaskPrintLabelMessage(string taskId)
    {
        TaskId = taskId;
    }
}

[Serializable, NetSerializable]
public enum NtrTaskUiKey
{
    Key
}
