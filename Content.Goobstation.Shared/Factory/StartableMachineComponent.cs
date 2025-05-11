// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 coderabbitai[bot] <136622811+coderabbitai[bot]@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DeviceLinking;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Factory;

/// <summary>
/// Machine that can be started with a signal.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(StartableMachineSystem))]
public sealed partial class StartableMachineComponent : Component
{
    /// <summary>
    /// Port you invoke to start the machine and raise <see cref="MachineStartedEvent"/>.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> StartPort = "Start";

    /// <summary>
    /// Controls <see cref="AutoStart"/>.
    /// Pulses toggle instead of setting true/false.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> AutoStartPort = "AutoStart";

    /// <summary>
    /// Whether starting will work when <c>TryAutoStart</c> is called.
    /// </summary>
    /// <remarks>
    /// Signals aren't predicted yet so not networked.
    /// </remarks>
    [DataField(serverOnly: true)]
    public bool AutoStart;

    /// <summary>
    /// Queues an auto start for the next tick.
    /// </summary>
    [DataField(serverOnly: true)]
    public bool AutoStartQueued;

    [DataField]
    public ProtoId<SourcePortPrototype> StartedPort = "Started";

    [DataField]
    public ProtoId<SourcePortPrototype> CompletedPort = "Completed";

    [DataField]
    public ProtoId<SourcePortPrototype> FailedPort = "Failed";
}

/// <summary>
/// Raised on the server when the start port is invoked while powered.
/// </summary>
[ByRefEvent]
public readonly record struct MachineStartedEvent();
