// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.Weapons.Multihit;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ActiveMultihitComponent : Component
{
    [ViewVariables]
    public float NextDamageMultiplier = 1f;

    [ViewVariables]
    public float DeletionAccumulator;

    [ViewVariables]
    public QueuedMultihitAttack? LastAttack;

    [DataField, AutoNetworkedField]
    public EntityUid? User;

    [DataField, AutoNetworkedField]
    public Queue<QueuedMultihitAttack> QueuedAttacks = new();
}

[Serializable, NetSerializable, DataDefinition]
public sealed partial class QueuedMultihitAttack
{
    [DataField]
    public TimeSpan AttackTime;

    [DataField]
    public float DamageMultiplier;

    [DataField]
    public NetEntity? Target;

    [DataField]
    public Vector2? Direction;
}

[Serializable, NetSerializable]
public sealed class ResetMultihitLastAttackEvent(NetEntity weapon) : EntityEventArgs
{
    public NetEntity Weapon = weapon;
}
