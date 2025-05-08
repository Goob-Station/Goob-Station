// SPDX-FileCopyrightText: 2025 Coenx-flex <coengmurray@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.AbilitySuppression;

[RegisterComponent, NetworkedComponent]
public sealed partial class MagicSuppressionComponent : Component
{
}

/// <summary>
///     Raised when you want to check if an item suppresses the users magic
/// </summary>
public sealed class CheckMagicSuppressionEvent : IInventoryRelayEvent
{
    public bool Cancelled;

    /// <summary>
    ///     Define what is blocking the spellcast
    /// </summary>
    public EntityUid Blocker;

    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
