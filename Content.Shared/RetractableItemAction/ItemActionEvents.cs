// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Actions;

namespace Content.Shared.RetractableItemAction;

/// <summary>
/// Raised when using the RetractableItem action.
/// </summary>
[ByRefEvent]
public sealed partial class OnRetractableItemActionEvent : InstantActionEvent;
