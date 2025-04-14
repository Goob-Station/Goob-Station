// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Spawn.Components;

/// <summary>
///     Ensures that related entity will be on station (like NTR or BSO lockers) and will be not duplicate.
///     If station have unique entity - item with this component will be deleted.
/// </summary>
[RegisterComponent]
public sealed partial class UniqueEntityCheckerComponent : Component
{
    /// <summary>
    ///     Name of marker in UniqueEntityMarker
    /// </summary>
    [DataField]
    public string? MarkerName;
}