using Content.Goobstation.Shared.Cult;

namespace Content.Goobstation.Server.Cult.Runes;

public sealed partial class CultRuneBehaviorTeleport : CultRuneBehavior
{
    public override bool IsValid(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, out string invalidReason)
    {
        if (!base.IsValid(ent, invokers, targets, out invalidReason))
            return false;

        return true;
    }

    public override void Invoke(IEntityManager ent, List<EntityUid> invokers, List<EntityUid> targets, EntityUid? owner = null)
    {
        // todo
    }
}
