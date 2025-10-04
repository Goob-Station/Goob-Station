using Content.Shared._Lavaland.Megafauna.Components;
using Content.Shared._Lavaland.Megafauna.Systems;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._Lavaland.Megafauna.Selectors;

/// <summary>
/// Performs an action and if required, tries to get target positions
/// from <see cref="MegafaunaAiTargetingComponent"/>.
/// </summary>
public sealed partial class PerformActionSelector : MegafaunaSelector
{
    [DataField]
    public EntProtoId ActionId;

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var actionSys = entMan.System<SharedActionsSystem>();
        var megafaunaSys = entMan.System<MegafaunaSystem>();

        if (!actionSys.TryGetActionById(args.BossEntity, ActionId, out var action))
        {
            args.Logger.Debug($"{entMan.ToPrettyString(args.BossEntity)}'s AI failed to get action with ID {ActionId}!");
            return FailDelay;
        }

        var ev = megafaunaSys.GetPerformEvent(args.BossEntity, action.Value.Owner);

        if (!actionSys.TryPerformAction(args.BossEntity, ev))
        {
            args.Logger.Debug($"{entMan.ToPrettyString(args.BossEntity)}'s AI failed to perform action {entMan.ToPrettyString(action.Value.Owner)} with ID {ActionId}!");
            return FailDelay;
        }

        return DelaySelector.Get(args);
    }
}
