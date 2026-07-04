// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Lumminal <81829924+Lumminal@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Goobstation.Shared.Shadowling.Components;
using Content.Shared._Shitmed.Damage;
using Content.Shared.Damage;
using Content.Shared.EntityEffects;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Chemistry.Components.SolutionManager;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.EntityEffects;

/// <summary>
/// HealthChange but unique to Shadowlings and Thralls
/// </summary>
public sealed partial class HealShadowlingSystem
    : EntityEffectSystem<SolutionContainerManagerComponent, HealShadowling>
{
    [Dependency] private readonly DamageableSystem _damage = default!;

    protected override void Effect(Entity<SolutionContainerManagerComponent> entity, ref EntityEffectEvent<HealShadowling> args)
    {
        // If slings get custom organs, I will remove all of this code tbf
        if (!HasComp<ShadowlingComponent>(entity.Owner) &&
            !HasComp<ThrallComponent>(entity.Owner))
        {
            return;
        }
        FixedPoint2 scale;
        scale = FixedPoint2.New(args.Scale);

        _damage.TryChangeDamage(
            entity.Owner,
            args.Effect.Damage * scale,
            args.Effect.IgnoreResistances,
            interruptsDoAfters: false,
            targetPart: TargetBodyPart.All,
            partMultiplier: 0.5f,
            splitDamage: SplitDamageBehavior.SplitEnsureAll,
            canMiss: false);
    }
}

[UsedImplicitly]
public sealed partial class HealShadowling : EntityEffectBase<HealShadowling>
{
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool IgnoreResistances = true;

    [DataField]
    public bool ScaleByQuantity;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-heal-sling", ("chance", Probability));
}
