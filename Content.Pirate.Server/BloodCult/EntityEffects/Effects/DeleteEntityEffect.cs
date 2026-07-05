// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.BloodCult.Components;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;

namespace Content.Server.BloodCult.EntityEffects.Effects;

/// <summary>
/// Entity effect that deletes the target entity when triggered.
/// Used for cleaning blood cult runes with reagents.
/// Only deletes basic runes (not tear veil or final summoning runes).
/// </summary>
[UsedImplicitly]
public sealed partial class DeleteEntityEffect : EntityEffect
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser raiser, float scale, EntityUid? user)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        // Only delete basic runes (not tear veil or final summoning runes)
        if (entMan.HasComponent<TearVeilComponent>(target) ||
            entMan.HasComponent<FinalSummoningRuneComponent>(target))
        {
            return;
        }

        // Only delete if it's a cleanable rune
        if (!entMan.HasComponent<CleanableRuneComponent>(target))
            return;

        // Delete the target entity
        entMan.QueueDeleteEntity(target);
    }
}
