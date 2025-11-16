using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Actions.Events;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.InternalResources.EntitySystems;

public sealed partial class SharedInternalResourcesActionSystem : EntitySystem
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
        if (args.Cancelled)
            return;

        var actionCost = GetActionCost(args.User, action.Comp.UseAmount);

        if (!_internalResources.TryGetResourceType(args.User, action.Comp.ResourceProto, out var data) || data.CurrentAmount < actionCost)
        {
            var typeName = Loc.GetString(_prototypeManager.Index(action.Comp.ResourceProto).Name);

            _popupSystem.PopupClient(Loc.GetString("internal-resources-action-no-resources", ("type", typeName)), args.User, args.User);
            args.Cancelled = true;
            return;
        }
    }

    private void OnActionPerformed(Entity<InternalResourcesActionComponent> action, ref ActionPerformedEvent args)
    {
        var actionCost = GetActionCost(args.Performer, action.Comp.UseAmount);

        _internalResources.TryUpdateResourcesAmount(args.Performer, action.Comp.ResourceProto, -actionCost);
    }

    private float GetActionCost(EntityUid user, float baseCost)
    {
        var modifierEv = new GetInternalResourcesCostModifierEvent(user);
        RaiseLocalEvent(user);

        return baseCost * modifierEv.Multiplier;
    }
}
