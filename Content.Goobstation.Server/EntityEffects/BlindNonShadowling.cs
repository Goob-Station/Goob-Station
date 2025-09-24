// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.EntityEffects;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
/// Inflicts blindness on non-shadowlings and non-thralls
/// </summary>
[UsedImplicitly]
public sealed partial class BlindNonShadowling : EntityEffect
{
    /// <inheritdoc/>
    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-blind-non-sling", ("chance", Probability));
    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args.EntityManager.HasComponent<ShadowlingComponent>(args.TargetEntity) ||
            args.EntityManager.HasComponent<ThrallComponent>(args.TargetEntity))
        {
            return;
        }

        if (!args.EntityManager.TryGetComponent<StatusEffectsComponent>(args.TargetEntity, out var statusEffects))
            return;

        var statusEffectsSystem = args.EntityManager.System<StatusEffectsSystem>();

        statusEffectsSystem.TryAddStatusEffect<TemporaryBlindnessComponent>(
            args.TargetEntity,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(3),
            true,
            statusEffects);
    }
}
