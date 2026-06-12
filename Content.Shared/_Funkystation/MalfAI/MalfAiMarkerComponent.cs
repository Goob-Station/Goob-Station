// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Marks a Station AI as a Malfunctioning AI antagonist.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiMarkerComponent : Component;
