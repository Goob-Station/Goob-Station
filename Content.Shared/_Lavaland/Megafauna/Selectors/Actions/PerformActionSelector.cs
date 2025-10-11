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

    [DataField]
    public string? EntityKey = "aggressor";

    [DataField]
    public string? CoordsKey = "aggressor";

    protected override float InvokeImplementation(MegafaunaCalculationBaseArgs args)
    {
        var entMan = args.EntityManager;
        var actionSys = entMan.System<SharedActionsSystem>();
        var megafaunaSys = entMan.System<MegafaunaSystem>();

        if (!actionSys.TryGetActionById(args.Entity, ActionId, out var action))
        {
            args.Logger.Debug($"{entMan.ToPrettyString(args.Entity)}'s AI failed to get action with ID {ActionId}!");
            return FailDelay;
        }

        var ev = megafaunaSys.GetPerformEvent(args.Entity, action.Value.Owner, EntityKey, CoordsKey);

        if (!actionSys.TryPerformAction(args.Entity, ev))
        {
            args.Logger.Debug($"{entMan.ToPrettyString(args.Entity)}'s AI failed to perform action {entMan.ToPrettyString(action.Value.Owner)} with ID {ActionId}!");
            return FailDelay;
        }

        return DelaySelector.Get(args);
    }
}
