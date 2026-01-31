using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.Data;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Actions.Components;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.EntitySystems;

public sealed class SharedInternalResourcesActionSystem : EntitySystem
{
    [Dependency] private readonly SharedInternalResourcesSystem _internalResources = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<InternalResourcesActionComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<InternalResourcesActionComponent, ActionPerformedEvent>(OnActionPerformed);
    }

    private void OnActionAttempt(Entity<InternalResourcesActionComponent> action, ref ActionAttemptEvent args)
    {
        if (args.Cancelled
            || !_internalResources.TryGetResourceType(args.User, action.Comp.ResourceProto, out var data)
            || !_prototypeManager.TryIndex(data.InternalResourcesType, out var proto))
            return;

        var actionCost = GetActionCost(args.User, data, action.Comp.UseAmount);

        if (data.CurrentAmount >= actionCost)
            return;

        var popup = action.Comp.DeficitPopup ?? proto.DeficitPopup;
        _popupSystem.PopupClient(Loc.GetString(popup), args.User, args.User);

        args.Cancelled = true;
    }

    private void OnActionPerformed(Entity<InternalResourcesActionComponent> action, ref ActionPerformedEvent args)
    {
        if (!_internalResources.TryGetResourceType(args.Performer, action.Comp.ResourceProto, out var data))
            return;

        var toggled = Comp<ActionComponent>(action).Toggled;
        var amount = !toggled ? action.Comp.UseAmount : action.Comp.AltUseAmount;

        var actionCost = GetActionCost(args.Performer, data, amount);

        _internalResources.TryUpdateResourcesAmount(args.Performer, action.Comp.ResourceProto, -actionCost);
    }

    private float GetActionCost(EntityUid user, InternalResourcesData data, float baseCost)
    {
        var modifierEv = new GetInternalResourcesCostModifierEvent(user, data);
        RaiseLocalEvent(user, ref modifierEv);

        return baseCost * modifierEv.Multiplier;
    }
}
