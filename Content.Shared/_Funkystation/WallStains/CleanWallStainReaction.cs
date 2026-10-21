using Content.Shared._Funkystation.WallStains.Components;
using Content.Shared.EntityEffects;

namespace Content.Shared._Funkystation.WallStains;

public sealed partial class CleanWallStainReaction : EntityEffect
{
    public override void RaiseEvent(EntityUid target, IEntityEffectRaiser args, float amount, EntityUid? origin)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!entMan.HasComponent<StainedWallComponent>(target))
            return;

        entMan.EventBus.RaiseLocalEvent(target, new CleanWallStainsEvent(transformToWater: true));
    }
}
