using Content.Shared.StatusEffectNew;
using Robust.Shared.Timing;

namespace Content.Shared._starcup.StatusEffectsNew;

public sealed partial class ComponentStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<ComponentStatusEffectComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
    }

    private void OnEffectApplied(Entity<ComponentStatusEffectComponent> entity, ref StatusEffectAppliedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var component = _componentFactory.GetComponent(entity.Comp.Component);
        if (!HasComp(args.Target, component.GetType()))
        {
            AddComp(args.Target, component);
        }
    }

    private void OnEffectRemoved(Entity<ComponentStatusEffectComponent> entity, ref StatusEffectRemovedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        var component = _componentFactory.GetComponent(entity.Comp.Component);
        RemComp(args.Target, component.GetType());
    }
}
