// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Objectives.Components;

/// <summary>
/// Limits the number of traitors that can have the same objective.
/// Checked by the prototype id, so only considers the exact same objectives.
/// </summary>
/// <remarks>
/// Only works for traitors so don't use for anything else.
/// </remarks>
[RegisterComponent, Access(typeof(ObjectiveLimitSystem))]
public sealed partial class ObjectiveLimitComponent : Component
{
    /// <summary>
    /// Max number of players
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public uint Limit;
}