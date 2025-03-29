using Content.Shared.EntityEffects;
using Content.Shared.Standing;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Wizard.Chemistry;

[UsedImplicitly]
public sealed partial class DropItemsEntityEffect : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Loc.GetString("reagent-effect-guidebook-drop-items", ("chance", Probability));
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        args.EntityManager.EventBus.RaiseLocalEvent(args.TargetEntity, new DropHandItemsEvent());
    }
}
