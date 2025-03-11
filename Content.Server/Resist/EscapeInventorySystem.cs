using Content.Server.Popups;
using Content.Shared.Storage;
using Content.Shared.Inventory;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Storage.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Events;
using Content.Shared.Resist;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server.Resist;

public sealed class EscapeInventorySystem : SharedEscapeInventorySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlockerSystem = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    /// <summary>
    /// DeltaV - action to cancel inventory escape
    /// </summary>
    [ValidatePrototypeId<EntityPrototype>]
    private readonly string _escapeCancelAction = "ActionCancelEscape";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CanEscapeInventoryComponent, EscapeInventoryCancelActionEvent>(OnCancelEscape);
    }

    public override void AttemptEscape(EntityUid user, EntityUid container, CanEscapeInventoryComponent component, float multiplier = 1f)
    {
        if (!_actionBlockerSystem.CanInteract(user, container))
            return;

        base.AttemptEscape(user, container, component, multiplier);

        // DeltaV - escape cancel action
        if (component.EscapeCancelAction is not { Valid: true })
            _actions.AddAction(user, ref component.EscapeCancelAction, _escapeCancelAction);
    }

    // DeltaV
    private void OnCancelEscape(EntityUid uid, CanEscapeInventoryComponent component, EscapeInventoryCancelActionEvent args)
    {
        if (component.DoAfter != null)
            _doAfterSystem.Cancel(component.DoAfter);

        _actions.RemoveAction(uid, component.EscapeCancelAction);
        component.EscapeCancelAction = null;
    }
}
