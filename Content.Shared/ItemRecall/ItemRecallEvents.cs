// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Content.Shared.Actions;

namespace Content.Shared.ItemRecall;

/// <summary>
/// Raised when using the ItemRecall action.
/// </summary>
[ByRefEvent]
public sealed partial class OnItemRecallActionEvent : InstantActionEvent;
