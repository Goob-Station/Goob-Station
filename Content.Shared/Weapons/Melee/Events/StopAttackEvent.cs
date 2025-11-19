// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Melee.Events;

[Serializable, NetSerializable]
public sealed class StopAttackEvent : EntityEventArgs
{
    public readonly NetEntity Weapon;

    public StopAttackEvent(NetEntity weapon)
    {
        Weapon = weapon;
    }
}
