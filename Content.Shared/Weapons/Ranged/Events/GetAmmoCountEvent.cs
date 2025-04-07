// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT
namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Raised on an AmmoProvider to request deets.
/// </summary>
[ByRefEvent]
public struct GetAmmoCountEvent
{
    public int Count;
    public int Capacity;
}