using Content.Server.Animals.Components;
using Content.Shared.Actions;

/// <summary>
/// Removes egg laying action when <see cref="EggLayerComponent"/> is removed from an entity.
/// </summary>
public sealed class TraumaEggLayerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EggLayerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<EggLayerComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.Action);
    }
}
