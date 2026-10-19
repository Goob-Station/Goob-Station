// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component for MalfAI doomsday objectives.
/// Used by the blacklist system to prevent conflicting objectives.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiDoomsdayMarkerComponent : Component
{
}
