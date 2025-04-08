// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Components;

/// <summary>
/// This component is required to make sure an entity is struck by the same lightning no more than once
/// </summary>
[RegisterComponent]
public sealed partial class StruckByLightningComponent : Component
{
    /// <summary>
    /// Indices of lightning beams that have struck this entity
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<uint> BeamIndices = new();

    /// <summary>
    /// This component is removed when it reaches zero.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public float Lifetime = 4f;
}