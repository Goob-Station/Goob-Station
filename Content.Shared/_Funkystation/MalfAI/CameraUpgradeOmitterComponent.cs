// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Excludes an entity from being considered a camera by the Malf AI camera upgrade overlay.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CameraUpgradeOmitterComponent : Component;
