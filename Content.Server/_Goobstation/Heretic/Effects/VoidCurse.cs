using Content.Server._Goobstation.Heretic.EntitySystems.PathSpecific;
using Content.Shared._Goobstation.Heretic.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Heretic.Effects;

public sealed partial class VoidCurse : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => "Inflicts void curse.";

    public override void Effect(EntityEffectBaseArgs args)
    {
        args.EntityManager.System<VoidCurseSystem>().DoCurse(args.TargetEntity);
    }
}
