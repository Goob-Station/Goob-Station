// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI.Factory;

/// <summary>
/// Marker component for the Malf AI robotics factory: a converted recycler that turns
/// crew fed into it into cyborgs subservient to the Malf AI.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RoboticsFactoryGridComponent : Component;
