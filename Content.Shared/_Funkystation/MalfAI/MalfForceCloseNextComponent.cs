// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Marker: forces the next door close action to skip collision checks.
/// Used during Malf AI lockdown grid ability.
/// </summary>
[RegisterComponent]
public sealed partial class MalfForceCloseNextComponent : Component;
