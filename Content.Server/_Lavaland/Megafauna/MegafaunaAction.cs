// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Lavaland.Megafauna.Components;
using JetBrains.Annotations;

namespace Content.Server._Lavaland.Megafauna;

/// <summary>
/// Seals a method to be invoked by some megafauna AI.
/// </summary>
/// <remarks>
/// If you want to make this action reusable, just make sure that at all steps
/// it doesn't require any specific components, and specify everything required
/// for the attack in DataFields.
///
/// In the future maybe we could even create a mega-ultra-boss for hehe-hahas
/// that casts all possible attacks at once on the player.
///
/// Pwetty pweeease, keep your code reusable so someone can make this funny idea in the future!!!
/// </remarks>
[ImplicitDataDefinitionForInheritors]
[MeansImplicitUse]
public abstract partial class MegafaunaAction
{
    /// <summary>
    /// Current weight for this action to be picked.
    /// </summary>
    [DataField] public float Weight = 1f;

    public virtual string Name => GetType().Name;

    /// <returns>
    /// Duration of this attack in seconds
    /// </returns>
    public abstract float Invoke(MegafaunaThinkBaseArgs args);
}

/// <summary>
/// Arguments that are used for Megafauna Actions and Conditions.
/// </summary>
public record struct MegafaunaThinkBaseArgs(
    EntityUid BossEntity,
    MegafaunaAiComponent AiComponent,
    IEntityManager EntityManager);
