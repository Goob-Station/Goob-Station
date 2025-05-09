// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;
using Content.Shared.Whitelist;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the objective entity has no blacklisted components.
/// Lets you check for incompatible objectives.
/// </summary>
[RegisterComponent, Access(typeof(ObjectiveBlacklistRequirementSystem))]
public sealed partial class ObjectiveBlacklistRequirementComponent : Component
{
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public EntityWhitelist Blacklist = new();
}