// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;
using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew.Components;
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

        if (!HasComp<StatusEffectContainerComponent>(entity.Owner))
            return;

        _status.TryUpdateStatusEffectDuration(
            entity.Owner,
            "TemporaryBlindness",
            out _,
            TimeSpan.FromSeconds(3)
            );
    }
}

[UsedImplicitly]
public sealed partial class BlindNonShadowling : EntityEffectBase<BlindNonShadowling>
{
    /// <inheritdoc/>
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-blind-non-sling", ("chance", Probability));
}
