using System.Linq;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

public sealed partial class PerformActionSelector : MegafaunaSelector
{
    [DataField]
    public EntProtoId ActionId;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var actionSys = entMan.System<SharedActionsSystem>();
        var actions = actionSys.GetActions(args.BossEntity).ToList();

        foreach (var (uid, comp) in actions)
        {
            var entityPrototypeId = entMan.GetComponent<MetaDataComponent>(uid).EntityPrototype?.ID;
            if (entityPrototypeId == null
                || entityPrototypeId != ActionId)
                continue;

            // TODO perform action here
            break;
        }

        return DelaySelector.Get(args);
    }
}
