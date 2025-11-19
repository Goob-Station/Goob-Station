// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Actions.Events;

/// <summary>
/// Raised on an action entity to get its event.
/// </summary>
[ByRefEvent]
public record struct ActionGetEventEvent(BaseActionEvent? Event = null);
