// SPDX-FileCopyrightText: 2024 EmoGarbage404 <retron404@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 John <35928781+sporkyz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Contraband;

/// <summary>
/// This is used for marking entities that are considered 'contraband' IC and showing it clearly in examine.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ContrabandSystem)), AutoGenerateComponentState]
public sealed partial class ContrabandComponent : Component
{
    /// <summary>
    ///     The degree of contraband severity this item is considered to have.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public ProtoId<ContrabandSeverityPrototype> Severity = "Restricted";

    /// <summary>
    ///     Which departments is this item restricted to?
    ///     By default, command and sec are assumed to be fine with contraband.
    ///     If null, no departments are allowed to use this.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public HashSet<ProtoId<DepartmentPrototype>> AllowedDepartments = new();

    /// <summary>
    ///     Which jobs is this item restricted to?
    ///     If empty, no jobs are allowed to use this beyond the allowed departments.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public HashSet<ProtoId<JobPrototype>> AllowedJobs = new();
}