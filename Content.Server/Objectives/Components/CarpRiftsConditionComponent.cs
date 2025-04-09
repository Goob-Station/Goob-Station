// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the dragon open and fully charge a certain number of rifts.
/// Depends on <see cref="NumberObjective"/> to function.
/// </summary>
[RegisterComponent, Access(typeof(CarpRiftsConditionSystem))]
public sealed partial class CarpRiftsConditionComponent : Component
{
    /// <summary>
    /// The number of rifts currently charged.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int RiftsCharged;
}