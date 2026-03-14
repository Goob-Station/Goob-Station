using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Robust.Shared.IoC;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons.AI.Conditions;

public sealed partial class DoodonHasOrderPrecondition : HTNPrecondition
{
    [DataField(required: true)]
    public DoodonOrderType Order;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!blackboard.TryGetValue(NPCBlackboard.Owner, out EntityUid uid, entMan))
            return false;

        return entMan.TryGetComponent(uid, out DoodonWarriorComponent? comp) && comp.Orders == Order;
    }
}
