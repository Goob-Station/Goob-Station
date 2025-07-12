using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.Chemistry.ReagentEffects;

[UsedImplicitly]
public sealed partial class ChemRestorePsionicReroll : EntityEffect
{
    [DataField]
    public bool BypassRoller;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-chem-restorereroll-psionic");

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs _
            || !args.EntityManager.TryGetComponent(args.TargetEntity, out PsionicComponent? psionicComp)
            || !psionicComp.Roller && !BypassRoller)
            return;

        psionicComp.CanReroll = true;
    }
}
