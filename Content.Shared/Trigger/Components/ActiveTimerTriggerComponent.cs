// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Trigger.Components;

/// <summary>
/// Component used for tracking active timers triggers.
/// Used internally for performance reasons.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveTimerTriggerComponent : Component;
