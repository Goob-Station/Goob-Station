// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.Weapons.Multishot;

/// <summary>
/// Component that allows guns to be shooted with another weapon by holding it in second hand
/// </summary>
[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MultishotComponent : Component
{
    /// <summary>
    /// Increasing spread when shooting with multiple hands
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpreadMultiplier = 1.5f;

    /// <summary>
    /// Uid of related weapon
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RelatedWeapon = default!;
}