// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BarryNorfolk <barrynorfolkman@protonmail.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Goobstation.Shared.NTR;

[RegisterComponent, NetworkedComponent]
public sealed partial class NtrTaskConsoleComponent : Component
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

    [DataField("slotId")]
    public string SlotId = "taskSlot";
}

[NetSerializable, Serializable]
public sealed class NtrTaskConsoleState : BoundUserInterfaceState
{
    public List<NtrTaskData> AvailableTasks { get; }
    public List<NtrTaskHistoryData> History { get; }
    public TimeSpan UntilNextSkip { get; }
    public HashSet<string> LockedTasks { get; }

    public NtrTaskConsoleState(
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
