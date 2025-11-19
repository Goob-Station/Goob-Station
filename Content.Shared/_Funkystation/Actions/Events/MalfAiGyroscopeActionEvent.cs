// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared._Funkystation.Actions;

// Action event for the Malf AI Gyroscope ability.
// This must be a world-targeted action to receive a map Target from the client.
public sealed partial class MalfAiGyroscopeActionEvent : WorldTargetActionEvent { }
