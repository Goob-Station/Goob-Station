using Content.Goobstation.Shared.TouchSpell;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;

public sealed partial class QueueDeleteTouchSpellEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not TouchSpellEffectArgs { } ts)
            return;

        ts.Origin.Comp.QueueDelete = true;
        ts.EntityManager.Dirty(ts.Origin.Owner, ts.Origin.Comp);
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
