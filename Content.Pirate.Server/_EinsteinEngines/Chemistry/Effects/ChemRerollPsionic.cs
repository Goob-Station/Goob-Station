using Content.Shared.Chemistry.Reagent;
using Content.Server.Psionics;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.ReagentEffects
{
    /// <summary>
    /// Rerolls psionics once.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class ChemRerollPsionic : EntityEffect
    {
        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
            => Loc.GetString("reagent-effect-guidebook-chem-reroll-psionic", ("chance", Probability));

        /// <summary>
        /// Reroll multiplier.
        /// </summary>
        [DataField("bonusMultiplier")]
        public float BonusMuliplier = 1f;

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs _)
                return;

            var psySys = args.EntityManager.EntitySysManager.GetEntitySystem<PsionicsSystem>();
            psySys.RerollPsionics(args.TargetEntity, bonusMuliplier: BonusMuliplier);
        }
    }
}
