// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// A filter that requires entities to have the exact same name as a set string.
/// </summary>
public sealed partial class NameFilter : AutomationFilter
{
    /// <summary>
    /// The name to require.
    /// This includes labels etc so "banana" will not match a labelled banana
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    public override bool IsAllowed(EntityUid uid)
    {
        return EntMan.GetComponent<MetaDataComponent>(uid).EntityName == Name;
    }
}
