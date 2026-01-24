using Content.Goobstation.Shared.Doodons;
using Content.Server.NPC;
using Content.Server.NPC.HTN.Preconditions;
using Robust.Shared.IoC;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Server.Doodons.AI.Conditions;

public sealed partial class DoodonHallFeralPrecondition : HTNPrecondition
{
    [DataField(required: true)]
    public bool Feral;

    public override bool IsMet(NPCBlackboard blackboard)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!blackboard.TryGetValue(NPCBlackboard.Owner, out EntityUid uid, entMan))
            return false;

        if (!entMan.TryGetComponent(uid, out DoodonComponent? doodon))
            return false;

        if (doodon.TownHall is not { } hallUid)
            return false;

        if (!entMan.TryGetComponent(hallUid, out DoodonTownHallFeralComponent? feral))
            return false;

        return feral.Feral == Feral;
    }
}
