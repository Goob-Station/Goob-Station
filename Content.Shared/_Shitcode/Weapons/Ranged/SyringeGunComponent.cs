// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Goobstation.Weapons.Ranged;

/// <summary>
///     Component that allows syringe-firing guns to uncap their injection limit on firing.
/// </summary>
[RegisterComponent]
public sealed partial class SyringeGunComponent : Component
{
    /// <summary>
    ///     Force fired projectiles to (not) pierce armor.
    ///     Doesn't apply if null.
    /// </summary>
    [DataField]
    public bool? PierceArmor;

    /// <summary>
    ///     Multiplies injection speed for fired syringes with SolutionInjectWhileEmbeddedComponent.
    /// </summary>
    [DataField]
    public float InjectionSpeedMultiplier = 1f;
}