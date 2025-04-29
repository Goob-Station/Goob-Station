// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.SprayPainter.Prototypes;

/// <summary>
/// Maps airlock style names to department ids.
/// </summary>
[Prototype("airlockDepartments")]
public sealed partial class AirlockDepartmentsPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Dictionary of style names to department ids.
    /// If a style does not have a department (e.g. external) it is set to null.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, ProtoId<DepartmentPrototype>> Departments = new();
}