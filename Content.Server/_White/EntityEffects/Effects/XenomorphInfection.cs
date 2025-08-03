using Content.Server.Body.Systems;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._White.EntityEffects.Effects;

[UsedImplicitly]
public sealed partial class XenomorphInfection : EntityEffect
{
    [DataField]
    public string SlotId = "xenomorph_larva";

    [DataField]
    public EntProtoId Prototype = "XenomorphInfection";

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => null;

    public override void Effect(EntityEffectBaseArgs args)
    {
        var bodySystem = args.EntityManager.System<BodySystem>();

        if (bodySystem.GetRootPartOrNull(args.TargetEntity) is not {} rootPart)
            return;

        var organ = args.EntityManager.Spawn(Prototype);
        bodySystem.TryCreateOrganSlot(rootPart.Entity, SlotId, out _, rootPart.BodyPart);
        if (!bodySystem.InsertOrgan(rootPart.Entity, organ, SlotId, rootPart.BodyPart))
            args.EntityManager.QueueDeleteEntity(organ);
    }
}
