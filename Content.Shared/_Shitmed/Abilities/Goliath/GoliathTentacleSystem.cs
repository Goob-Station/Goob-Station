using Content.Shared.Actions;

namespace Content.Shared._Shitmed.GoliathTentacle;

internal sealed class GoliathTentacleSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<GoliathTentacleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GoliathTentacleComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, GoliathTentacleComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ActionEntity, component.Action);
    }

    private void OnShutdown(EntityUid uid, GoliathTentacleComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ActionEntity);
    }
}
