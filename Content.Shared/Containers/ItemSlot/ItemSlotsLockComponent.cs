// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.GameStates;

namespace Content.Shared.Containers.ItemSlots;

/// <summary>
/// Updates the relevant ItemSlots locks based on <see cref="LockComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemSlotsLockComponent : Component
{
    [DataField(required: true)]
    public List<string> Slots = new();
}
