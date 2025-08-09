// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 BombasterDS <deniskaporoshok@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Common.Weapons.Multishot;

/// <summary>
/// This component allows guns to be shot with another such gun at the same time by holding them in both hands.
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultishotComponent : Component
{
    /// <summary>
    /// Shows that this entity is affected with multishot debuffs.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool MultishotAffected;

    /// <summary>
    /// The chance of bullet fired of the gun while dual-wielding getting its BulletImpassable fixture layer removed.
    /// This effectively means that the bullet won't collide with people, but has no effect on hitscans (like lasers).
    /// </summary>
    [DataField]
    public float MissChance = 0.2f;

    /// <summary>
    /// Spread multiplier of the gun while dual-wielding.
    /// </summary>
    [DataField]
    public float SpreadMultiplier = 1.5f;

    /// <summary>
    /// Flat spread increase of the gun while dual-wielding.
    /// </summary>
    [DataField]
    public float SpreadAddition = 5f;

    /// <summary>
    /// This is Common, and we can't import DamageSpecifier here, so we shitcode it instead.
    /// Amount of damage applied to hands when firing this gun with another one. Doesn't damage the shooter itself.
    /// </summary>
    [DataField]
    public float HandDamageAmount;

    /// <summary>
    /// <see cref="HandDamageAmount"/> Type of the said damage.
    /// </summary>
    [DataField]
    public string HandDamageType = "Blunt";

    /// <summary>
    ///  Stamina damage applied to the entity when firing this gun with another one.
    /// </summary>
    [DataField]
    public float StaminaDamage;

    [DataField]
    public string ExamineMessage = "multishot-component-examine";
}
