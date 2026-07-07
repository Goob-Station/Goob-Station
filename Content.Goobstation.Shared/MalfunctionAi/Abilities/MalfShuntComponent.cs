// SPDX-FileCopyrightText: 2026 Jonikibaka <153797633+Jonikibaka@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.MalfunctionAi;

/// <summary>
/// Tuning for the Shunt to APC ability.
/// </summary>
[RegisterComponent]
public sealed partial class MalfShuntComponent : Component
{
    /// <summary>Entity spawned inside an APC when the AI shunts its consciousness into it.</summary>
    [DataField] public EntProtoId ShuntEntity = "MalfShuntedAi";
}
