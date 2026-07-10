// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;

namespace Content.Shared.Spider;

public abstract class SharedSpiderSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpiderComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<SpiderComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnInit(EntityUid uid, SpiderComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.Action, component.WebAction, uid);

        // Pirate: Starlight terror spiders can unlock one web building action.
        if (component.HasBuilding)
            _action.AddAction(uid, ref component.BuildingAction, component.BuildingActionProto, uid);
    }

    private void OnShutdown(EntityUid uid, SpiderComponent component, ComponentShutdown args)
    {
        _action.RemoveAction(uid, component.Action);
        _action.RemoveAction(uid, component.BuildingAction);
    }
}
