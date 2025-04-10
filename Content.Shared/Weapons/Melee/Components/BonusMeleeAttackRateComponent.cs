// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedMeleeWeaponSystem))]
public sealed partial class BonusMeleeAttackRateComponent : Component
{
    /// <summary>
    /// The value added onto the attack rate of a melee weapon
    /// </summary>
    [DataField("flatModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float FlatModifier;

    /// <summary>
    /// A value that is multiplied by the attack rate of a melee weapon
    /// </summary>
    [DataField("multiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 1;

    /// <summary>
    /// A value that is added on to a weapon's heavy windup time.
    /// </summary>
    [DataField("heavyWindupFlatModifier"), ViewVariables(VVAccess.ReadWrite)]
    public float HeavyWindupFlatModifier;

    /// <summary>
    /// A value that is multiplied by a weapon's heavy windup time
    /// </summary>
    [DataField("heavyWindupMultiplier"), ViewVariables(VVAccess.ReadWrite)]
    public float HeavyWindupMultiplier = 1;
}