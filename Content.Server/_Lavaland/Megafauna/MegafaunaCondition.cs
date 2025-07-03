// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Server._Lavaland.Megafauna;

/// <summary>
/// Represents a condition that is checked before making some specific MegafaunaAction.
/// </summary>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaCondition
{
    public virtual string Name => GetType().Name;

    /// <returns>
    /// Has this condition passed or not
    /// </returns>
    public abstract bool Check(MegafaunaThinkBaseArgs args);
}
