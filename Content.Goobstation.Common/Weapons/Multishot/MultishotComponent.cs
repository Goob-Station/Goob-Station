// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Weapons.Multishot;

/// <summary>
/// Component that allows guns to be shooted with another weapon by holding it in second hand
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultishotComponent : Component
{
    /// <summary>
    /// Increasing spread when shooting with multiple hands.
    /// </summary>
    [DataField]
    public float SpreadMultiplier = 1.5f;

    /// <summary>
    /// Shows that this entity is affected with multishot debuffs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool MultishotAffected = false;

    /// <summary>
    /// Makes damage to hand without damaging entity.
    /// Cry: Can't use DamageSpecifier because this component in Common.
    /// </summary>
    [DataField]
    public float HandDamage = 0f;

    /// <summary>
    /// Flat spread increase.
    /// </summary>
    [DataField]
    public float FlatSpreadAddition = 5f;

    /// <summary>
    /// Deal stamina damage on dual welding.
    /// </summary>
    [DataField]
    public float StaminaDamage = 0f;
}
