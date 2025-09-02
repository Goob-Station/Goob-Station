using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Organ;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Toggleable;

namespace Content.Goobstation.Shared.Augments;

public sealed class AugmentActionSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly AugmentSystem _augment = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AugmentActionComponent, OrganEnableChangedEvent>(OnOrganEnableChanged);
        SubscribeLocalEvent<AugmentActionComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<AugmentActionComponent, ToggleActionEvent>(OnToggleAction);
    }

    private void OnOrganEnableChanged(Entity<AugmentActionComponent> ent, ref OrganEnableChangedEvent args)
    {
        if (_augment.GetBody(ent) is not {} body)
            return;

        EnsureComp<ActionsContainerComponent>(ent);
        if (args.Enabled)
        {
            _actions.AddAction(body, ref ent.Comp.ActionEntity, ent.Comp.Action, ent);
        }
        else
        {
            _actions.SetToggled(ent.Comp.ActionEntity, false);
            _actions.RemoveAction(body, ent.Comp.ActionEntity);
        }
    }

    private void OnToggled(Entity<AugmentActionComponent> ent, ref ItemToggledEvent args)
    {
        _actions.SetToggled(ent.Comp.ActionEntity, args.Activated);
    }

    private void OnToggleAction(Entity<AugmentActionComponent> ent, ref ToggleActionEvent args)
    {
        _toggle.Toggle(ent.Owner, user: args.Performer);
        args.Handled = true;
    }
}
