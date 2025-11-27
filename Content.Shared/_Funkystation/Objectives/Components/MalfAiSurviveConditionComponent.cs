// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;

namespace Content.Shared._Funkystation.Objectives.Components;

/// <summary>
/// Condition for MalfAI survive objective.
/// Completed when the AI entity still exists and has not been destroyed or carded at round end.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiSurviveConditionComponent : Component { }
