// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.ThrowableBlocker;

/// <summary>
/// Added to objects that can be blocked by ThrowableBlockerComponent
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThrowableBlockedComponent : Component
{
    [DataField]
    public BlockBehavior Behavior = BlockBehavior.KnockOff;

    /// <summary>
    /// How much damage will the entity take on block if Behavior is Damage
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = new();
}

public enum BlockBehavior : byte
{
    KnockOff = 0,
    Damage,
    Destroy,
}