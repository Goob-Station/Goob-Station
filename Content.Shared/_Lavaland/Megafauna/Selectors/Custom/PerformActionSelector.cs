using System.Linq;
using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Performs an action and if required, tries to get target positions
/// from <see cref="MegafaunaTargetingComponent"/>.
/// </summary>
public sealed partial class PerformActionSelector : MegafaunaSelector
{
    [DataField]
    public EntProtoId ActionId;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var actionSys = entMan.System<SharedActionsSystem>();
        var actions = actionSys.GetActions(args.BossEntity).ToList();

        foreach (var (uid, _) in actions)
        {
            var entityPrototypeId = entMan.GetComponent<MetaDataComponent>(uid).EntityPrototype?.ID;
            if (entityPrototypeId == null
                || entityPrototypeId != ActionId)
                continue;

            var netAction = entMan.GetNetEntity(uid);
            var netTarget = entMan.GetNetEntity(entMan.GetComponentOrNull<MegafaunaTargetingComponent>(args.BossEntity)?.TargetEntity);
            var netCoords = entMan.GetNetCoordinates(entMan.GetComponentOrNull<MegafaunaTargetingComponent>(args.BossEntity)?.TargetCoordinate);
            var ev = new RequestPerformActionEvent(netAction, netTarget, netCoords);

            actionSys.TryPerformAction(args.BossEntity, ev);
            break;
        }

        return DelaySelector.Get(args);
    }
}
