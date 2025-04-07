// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Lock;
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