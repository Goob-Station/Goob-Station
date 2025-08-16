using Content.Shared._Lavaland.Megafauna.Systems;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Conditions;

public sealed partial class ActionAvailableCondition : MegafaunaCondition
{
    [DataField(required: true)]
    public EntProtoId ActionId;

    public override bool EvaluateImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var actionSys = entMan.System<SharedActionsSystem>();
        var megafaunaSys = entMan.System<MegafaunaSystem>();

        if (!actionSys.TryGetActionById(args.BossEntity, ActionId, out var action))
            return false;

        var ev = megafaunaSys.GetPerformEvent(args.BossEntity, action.Value.Owner);
        return actionSys.CanPerformAction(args.BossEntity, action.Value, ev);
    }
}
