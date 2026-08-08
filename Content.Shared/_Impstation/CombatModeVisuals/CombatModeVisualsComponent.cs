// SPDX-FileCopyrightText: 2026 Impstation contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Impstation.CombatModeVisuals;

/// <summary>
/// Allows the use of unique sprites for combat mode
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CombatModeVisualsComponent : Component
{
    [DataField]
    public bool HideBaseLayer;
}
