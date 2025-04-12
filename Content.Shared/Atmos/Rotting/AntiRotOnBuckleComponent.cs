// SPDX-FileCopyrightText: 2024 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Atmos.Rotting;

/// <summary>
/// Perishable entities buckled to an entity with this component will stop rotting.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AntiRotOnBuckleComponent : Component
{
    /// <summary>
    /// Does this component require power to function.
    /// </summary>
    [DataField("requiresPower"), ViewVariables(VVAccess.ReadWrite)]
    public bool RequiresPower = true;

    /// <summary>
    /// Whether this component is active or not.
    /// </summarY>
    [ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = true;
}