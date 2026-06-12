// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Server._Funkystation.Objectives.Systems;

namespace Content.Server._Funkystation.Objectives.Components;

/// <summary>
/// Component for handling protect target selection, prioritizing traitors and traitor targets.
/// </summary>
[RegisterComponent, Access(typeof(MalfAiPickProtectTargetSystem))]
public sealed partial class MalfAiPickProtectTargetComponent : Component
{
}
