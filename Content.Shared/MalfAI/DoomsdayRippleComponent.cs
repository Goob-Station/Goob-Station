// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Shared, networked marker component that indicates an active Malf AI doomsday ripple.
/// Clients render the visual overlay based on this marker's data.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DoomsdayRippleComponent : Component
{
    /// <summary>
    /// Server real time when the ripple started.
    /// </summary>
    [ViewVariables]
    public TimeSpan StartTime;

    /// <summary>
    /// Duration of the visual effect. Default 20 seconds.
    /// </summary>
    [ViewVariables]
    public TimeSpan VisualDuration = TimeSpan.FromSeconds(20);

    /// <summary>
    /// Max visual range in tiles. Default 300.
    /// </summary>
    [ViewVariables]
    public float VisualRange = 300f;
}
