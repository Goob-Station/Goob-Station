// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the Malfunctioning AI game rule.
/// Handles the setup and management of Malf AI antagonist rounds.
/// </summary>
[RegisterComponent, EntityCategory("GameRules")]
public sealed partial class MalfAiRuleComponent : Component;
