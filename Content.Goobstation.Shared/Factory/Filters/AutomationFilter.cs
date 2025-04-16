// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.Factory.Filters;

/// <summary>
/// A filter that can be used to narrow down items found by automated machines.
/// </summary>
[ImplicitDataDefinitionForInheritors]
public abstract partial class AutomationFilter
{
    [Dependency] public readonly IEntityManager EntMan = default!;

    public void Initialize()
    {
        IoCManager.InjectDependencies(this);
    }

    /// <summary>
    /// Returns true if an item is allowed by the filter.
    /// </summary>
    public abstract bool IsAllowed(EntityUid uid);

    public bool IsDenied(EntityUid uid) => !IsAllowed(uid);
}
