using Content.Goobstation.Shared.Devil.Components;
using Content.Shared.EntityEffects;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Goobstation.Shared.Devil.EntityEffects;

public sealed partial class HellstepEffect : EntityEffect
{
    [DataField]
    public EntProtoId FirePrototype = "HereticFireAA";

    [DataField]
    public EntProtoId LavaPrototype = "FloorLavaEntityTemporary";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var uid = args.TargetEntity;
        var entMan = args.EntityManager;

        var hellstep = entMan.EnsureComponent<HellstepComponent>(uid); // EntEffect can't access EntSystem ig
        hellstep.FirePrototype = FirePrototype;
        hellstep.LavaPrototype = LavaPrototype;
    }
}
