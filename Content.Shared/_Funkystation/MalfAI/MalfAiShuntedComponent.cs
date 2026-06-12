// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 Goob-Station
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.MalfAI;

/// <summary>
/// Added to a Malf AI when it has shunted its mind into an APC.
/// Tracks the original core holder and the return action.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiShuntedComponent : Component
{
    [DataField]
    public EntityUid? CoreHolder;

    [DataField]
    public EntityUid? ReturnAction;

    /// <summary>
    /// Set while the shunt system moves the brain between holders (APC to APC),
    /// so the leave-cleanup keeps the shunt state instead of wiping it.
    /// </summary>
    public bool Transferring;
}
