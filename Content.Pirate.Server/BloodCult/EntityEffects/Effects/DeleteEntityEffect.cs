// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.BloodCult.Components;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntityEffects.Effects;

/// <summary>
/// Entity effect that deletes the target entity when triggered.
/// Used for cleaning blood cult runes with reagents.
/// Only deletes basic runes (not tear veil or final summoning runes).
/// </summary>
[UsedImplicitly]
public sealed partial class DeleteEntityEffect : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return null; // Not shown in guidebook
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        // Only delete basic runes (not tear veil or final summoning runes)
        if (args.EntityManager.HasComponent<TearVeilComponent>(args.TargetEntity) ||
            args.EntityManager.HasComponent<FinalSummoningRuneComponent>(args.TargetEntity))
        {
            return;
        }

        // Only delete if it's a cleanable rune
        if (!args.EntityManager.HasComponent<CleanableRuneComponent>(args.TargetEntity))
            return;

        // Delete the target entity
        args.EntityManager.QueueDeleteEntity(args.TargetEntity);
    }
}

