// SPDX-FileCopyrightText: 2024 EmoGarbage404 <retron404@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 John <35928781+sporkyz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared.Contraband;

/// <summary>
/// This is a prototype for defining the degree of severity for a particular <see cref="ContrabandComponent"/>
/// </summary>
[Prototype]
public sealed partial class ContrabandSeverityPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Text shown for this severity level when the contraband is examined.
    /// </summary>
    [DataField]
    public LocId ExamineText;

    /// <summary>
    /// When examining the contraband, should this take into account the viewer's departments and job?
    /// </summary>
    [DataField]
    public bool ShowDepartmentsAndJobs;
}