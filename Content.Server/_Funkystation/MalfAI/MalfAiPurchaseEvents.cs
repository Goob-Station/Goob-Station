// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Server._Funkystation.MalfAI;

/// <summary>
/// Raised on the Malf AI when it purchases the Syndicate Radio upgrade.
/// </summary>
[DataDefinition]
public sealed partial class MalfAiSyndicateKeysUnlockedEvent : EntityEventArgs;

/// <summary>
/// Raised on the Malf AI when it purchases the Camera Upgrade.
/// </summary>
[DataDefinition]
public sealed partial class MalfAiCameraUpgradeUnlockedEvent : EntityEventArgs;

/// <summary>
/// Raised on the Malf AI when it purchases Camera Microphones.
/// </summary>
[DataDefinition]
public sealed partial class MalfAiCameraMicrophonesUnlockedEvent : EntityEventArgs;
