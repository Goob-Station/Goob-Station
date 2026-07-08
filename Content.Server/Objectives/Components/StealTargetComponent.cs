// SPDX-FileCopyrightText: 2023 Colin-Tel <113523727+Colin-Tel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Flareguy <78941145+Flareguy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server.Objectives.Components.Targets;

/// <summary>
/// Allows an object to become the target of a StealCollection  objection
/// </summary>
[RegisterComponent]
public sealed partial class StealTargetComponent : Component
{
    /// <summary>
    /// The theft group to which this item belongs.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public string StealGroup;
}