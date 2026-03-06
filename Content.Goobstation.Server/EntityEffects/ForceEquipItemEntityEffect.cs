using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Server.EntityEffects;
public sealed partial class ForceEquipItemEntityEffect : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        // TODO
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;
}
