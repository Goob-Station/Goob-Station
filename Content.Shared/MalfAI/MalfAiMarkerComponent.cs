// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Marker component placed on the Station AI when it becomes a Malfunctioning AI antagonist.
/// Used to gate special interactions (e.g., APC CPU siphoning) without affecting visuals like EMAG.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiMarkerComponent : Component
{
}
