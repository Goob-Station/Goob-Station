// SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Client.Atmos.Visualizers;

/// <summary>
/// Holds 2 pairs of states. The idle/running pair controls animation, while
/// the ready / full pair controls the color of the light.
/// </summary>
[RegisterComponent]
public sealed partial class PortableScrubberVisualsComponent : Component
{
    [DataField("idleState", required: true)]
    public string IdleState = default!;

    [DataField("runningState", required: true)]
    public string RunningState = default!;

    /// Powered and not full
    [DataField("readyState", required: true)]
    public string ReadyState = default!;

    /// Powered and full
    [DataField("fullState", required: true)]
    public string FullState = default!;
}