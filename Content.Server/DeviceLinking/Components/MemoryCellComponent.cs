// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.DeviceLinking.Components;

/// <summary>
/// Memory cell that sets the output to the input when enabled.
/// </summary>
[RegisterComponent, Access(typeof(MemoryCellSystem))]
public sealed partial class MemoryCellComponent : Component
{
    /// <summary>
    /// Name of the input port.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> InputPort = "MemoryInput";

    /// <summary>
    /// Name of the enable port.
    /// </summary>
    [DataField]
    public ProtoId<SinkPortPrototype> EnablePort = "MemoryEnable";

    /// <summary>
    /// Name of the output port.
    /// </summary>
    [DataField]
    public ProtoId<SourcePortPrototype> OutputPort = "Output";

    // State
    [DataField]
    public SignalState InputState = SignalState.Low;

    [DataField]
    public SignalState EnableState = SignalState.Low;

    [DataField]
    public bool LastOutput;
}