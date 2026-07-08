// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.shared.Chemistry;

[UsedImplicitly]
public sealed partial class TakeStaminaDamage : EntityEffect
{
    /// <summary>
    /// How much stamina damage to take.
    /// </summary>
    [DataField]
    public int Amount = 10;

    /// <summary>
    /// Whether stamina damage should be applied immediately
    /// </summary>
    [DataField]
    public bool Immediate;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-deal-stamina-damage",
            ("immediate", Immediate),
            ("amount", MathF.Abs(Amount)),
            ("chance", Probability),
            ("deltasign", MathF.Sign(Amount)));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale != 1f)
                return;
        }

        args.EntityManager.System<SharedStaminaSystem>()
            .TakeStaminaDamage(args.TargetEntity, Amount, visual: false, immediate: Immediate);
    }
}
