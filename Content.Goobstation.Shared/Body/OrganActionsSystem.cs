using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;

namespace Content.Goobstation.Shared.Body;

public sealed class OrganActionsSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OrganActionsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OrganActionsComponent, OrganEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<OrganActionsComponent, OrganDisabledEvent>(OnDisabled);
    }

    private void OnMapInit(Entity<OrganActionsComponent> ent, ref MapInitEvent args)
    {
        var actions = EnsureComp<ActionsContainerComponent>(ent);
        foreach (var id in ent.Comp.Actions)
        {
            _actionContainer.AddAction(ent, id, actions);
        }
    }

    private void OnEnabled(Entity<OrganActionsComponent> ent, ref OrganEnabledEvent args)
    {
        if (args.Organ.Comp.Body is {} body)
            _actions.GrantContainedActions(body, ent.Owner);
    }

    private void OnDisabled(Entity<OrganActionsComponent> ent, ref OrganDisabledEvent args)
    {
        if (args.Organ.Comp.Body is {} body)
            _actions.RemoveProvidedActions(body, ent.Owner);
    }
}
