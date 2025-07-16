// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using JetBrains.Annotations;

namespace Content.Shared._Lavaland.Megafauna;

/// <summary>
/// Seals a method to be invoked by some megafauna AI.
/// </summary>
/// <remarks>
/// If you want to make this action reusable, just make sure that at all steps
/// it doesn't require any specific components, and specify everything required
/// for the attack in DataFields.
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaAction
{
    public virtual string Name => GetType().Name;

    /// <returns>
    /// Duration of this attack in seconds
    /// </returns>
    public abstract float Invoke(MegafaunaCalculationBaseArgs args);
}
