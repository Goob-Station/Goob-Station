// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BarryNorfolk <barrynorfolkman@protonmail.com>
// SPDX-FileCopyrightText: 2025 BeBright <98597725+be1bright@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 LuciferEOS <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 LuciferMkshelter <stepanteliatnik2022@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
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
    [DataField]
    public SoundSpecifier SkipSound = new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");

    /// <summary>
    /// The sound made when bounty skipping is denied due to lacking access.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_two.ogg");

    /// <summary>
    /// The time at which the console will be able to print a label again.
    /// </summary>
    [DataField]
    public TimeSpan NextPrintTime = TimeSpan.Zero;

    /// <summary>
    /// The time at which the console will be able to make sound again.
    /// </summary>
    [DataField]
    public TimeSpan NextSoundTime = TimeSpan.Zero;

    /// <summary>
    /// The id of the label entity spawned by the print label button.
    /// </summary>
    [DataField]
    public string TaskLabelId = "Paper";

    /// <summary>
    /// The time between prints.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The time between various triggeable things.
    /// </summary>
    [DataField]
    public SoundSpecifier PrintSound = new SoundPathSpecifier("/Audio/Machines/printer.ogg");

    [DataField]
    public HashSet<string> ActiveTaskIds = new();

    [DataField]
    public string SlotId = "taskSlot";
}

[NetSerializable, Serializable]
public sealed class NtrTaskConsoleState(
    List<NtrTaskData> available,
    List<NtrTaskHistoryData> history,
    TimeSpan skipTime,
    HashSet<string>? locked = null)
    : BoundUserInterfaceState
{
    public List<NtrTaskData> AvailableTasks { get; } = available;
    public List<NtrTaskHistoryData> History { get; } = history;
    public TimeSpan UntilNextSkip { get; } = skipTime;
    public HashSet<string> LockedTasks { get; } = locked ?? new HashSet<string>();
}

[Serializable, NetSerializable]
public sealed class TaskSkipMessage(string taskId) : BoundUserInterfaceMessage
{
    public string TaskId = taskId;
}
[Serializable, NetSerializable]
public sealed class TaskPrintLabelMessage(string taskId) : BoundUserInterfaceMessage
{
    public string TaskId = taskId;
}

[Serializable, NetSerializable]
public enum NtrTaskUiKey
{
    Key
}
