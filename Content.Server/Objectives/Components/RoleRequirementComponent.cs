// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Objectives.Systems;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player's mind matches a whitelist.
/// Typical use is checking for (antagonist) roles.
/// </summary>
[RegisterComponent, Access(typeof(RoleRequirementSystem))]
public sealed partial class RoleRequirementComponent : Component
{
    /// <summary>
    /// Mind role component whitelist.
    /// </summary>
    [DataField(required: true, customTypeSerializer: typeof(CustomHashSetSerializer<string, ComponentNameSerializer>))]
    public HashSet<string> Roles = new();
}
