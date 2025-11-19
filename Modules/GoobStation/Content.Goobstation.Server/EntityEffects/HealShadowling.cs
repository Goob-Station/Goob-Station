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
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

/// <summary>
/// HealthChange but unique to Shadowlings and Thralls
/// </summary>
[UsedImplicitly]
public sealed partial class HealShadowling : EntityEffect
{
    [DataField]
    public DamageSpecifier Damage = default!;

    [DataField]
    public bool IgnoreResistances = true;

    [DataField]
    public bool ScaleByQuantity;
    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) =>
        Loc.GetString("reagent-effect-guidebook-heal-sling", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // If slings get custom organs, I will remove all of this code tbf
        if (!args.EntityManager.HasComponent<ShadowlingComponent>(args.TargetEntity) &&
            !args.EntityManager.HasComponent<ThrallComponent>(args.TargetEntity))
        {
            return;
        }

        var scale = FixedPoint2.New(1);

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            scale = ScaleByQuantity ? reagentArgs.Quantity * reagentArgs.Scale : reagentArgs.Scale;
        }

        args.EntityManager.System<DamageableSystem>()
            .TryChangeDamage(
                args.TargetEntity,
                Damage * scale,
                IgnoreResistances,
                interruptsDoAfters: false,
                targetPart: TargetBodyPart.All,
                partMultiplier: 0.5f,
                splitDamage: SplitDamageBehavior.SplitEnsureAll,
                canMiss: false);
    }
}
