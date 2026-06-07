using Content.Shared._Shitmed.Body.Organ;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Robust.Shared.Network;

namespace Content.Goobstation.Shared.Body;

public sealed class OrganActionsSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private EntityQuery<OrganComponent> _organQuery;

    public override void Initialize()
    {
        base.Initialize();

        _organQuery = GetEntityQuery<OrganComponent>();

        SubscribeLocalEvent<OrganActionsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OrganActionsComponent, OrganAddedEvent>(OnAdded);
        SubscribeLocalEvent<OrganActionsComponent, OrganEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<OrganActionsComponent, OrganDisabledEvent>(OnDisabled);
        SubscribeLocalEvent<OrganActionsComponent, OrganRemovedEvent>(OnRemoved);
    }

    private void OnMapInit(Entity<OrganActionsComponent> ent, ref MapInitEvent args)
    {
        var actions = EnsureComp<ActionsContainerComponent>(ent);
        foreach (var id in ent.Comp.Actions)
        {
            _actionContainer.AddAction(ent, id, actions);
        }
    }

    private void OnAdded(Entity<OrganActionsComponent> ent, ref OrganAddedEvent args)
    {
        // container shit chuds out
        if (_net.IsClient)
            return;

        if (TryComp<ActionsContainerComponent>(ent, out var container))
            _actions.GrantContainedActions(args.Body, (ent, container));
    }

    private void OnEnabled(Entity<OrganActionsComponent> ent, ref OrganEnabledEvent args)
    {
        if (!_organQuery.TryComp(ent, out var organ) || organ.Body is not { } body)
            return;

        if (TryComp<ActionsContainerComponent>(ent, out var container))
            _actions.GrantContainedActions(body, (ent, container));
    }

    private void OnDisabled(Entity<OrganActionsComponent> ent, ref OrganDisabledEvent args)
    {
        if (args.Organ.Comp.Body is {} body)
            _actions.RemoveProvidedActions(body, ent.Owner);
    }

    private void OnRemoved(Entity<OrganActionsComponent> ent, ref OrganRemovedEvent args)
    {
        if (args.OldBody is {} body)
            _actions.RemoveProvidedActions(body, ent.Owner);
    }
}
