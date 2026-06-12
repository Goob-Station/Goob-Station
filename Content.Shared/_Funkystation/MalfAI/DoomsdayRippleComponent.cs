// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Tracks the expanding doomsday ripple effect state.
/// </summary>
[RegisterComponent]
public sealed partial class DoomsdayRippleComponent : Component
{
    [DataField]
    public TimeSpan StartTime;

    [DataField]
    public TimeSpan VisualDuration = TimeSpan.FromSeconds(20);

    [DataField]
    public float VisualRange = 300f;
}
