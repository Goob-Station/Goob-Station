// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Item;

/// <summary>
/// Raised directed on an entity when its item size / shape changes.
/// </summary>
[ByRefEvent]
public struct ItemSizeChangedEvent(EntityUid Entity)
{
    public EntityUid Entity = Entity;
}
