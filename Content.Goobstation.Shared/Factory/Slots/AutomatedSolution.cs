// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Factory.Slots;

/// <summary>
/// An automated solution that can be used by liquid pumps.
/// </summary>
public sealed partial class AutomatedSolution : AutomationSlot
{
    [DataField(required: true)]
    public string SolutionName;
}
