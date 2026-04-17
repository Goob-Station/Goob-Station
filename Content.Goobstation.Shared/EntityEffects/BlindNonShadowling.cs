// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Humanoid;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
/// Inflicts blindness on non-shadowlings and non-thralls
/// </summary>
// todo migrate. or just kill slings i  stg.
public sealed partial class BlindNonShadowlingSystem : EntityEffectSystem<HumanoidAppearanceComponent, BlindNonShadowling>
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    protected override void Effect(Entity<HumanoidAppearanceComponent> entity, ref EntityEffectEvent<BlindNonShadowling> args)
    {
        if (HasComp<ShadowlingComponent>(entity.Owner) ||
            HasComp<ThrallComponent>(entity.Owner))
        {
            return;
        }

        if (!TryComp<StatusEffectsComponent>(entity.Owner, out var statusEffects))
            return;

        _status.TryAddStatusEffect<TemporaryBlindnessComponent>(
            entity.Owner,
            "TemporaryBlindness",
            TimeSpan.FromSeconds(3),
            true,
            statusEffects);
    }
}

[UsedImplicitly]
public sealed partial class BlindNonShadowling : EntityEffectBase<BlindNonShadowling>
{
    /// <inheritdoc/>
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-blind-non-sling", ("chance", Probability));
}
