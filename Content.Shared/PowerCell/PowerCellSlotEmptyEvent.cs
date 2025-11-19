// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.PowerCell;

/// <summary>
/// Raised directed on an entity when its active power cell has no more charge to supply.
/// </summary>
[ByRefEvent]
public readonly record struct PowerCellSlotEmptyEvent;
