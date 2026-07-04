using Content.Shared.StatusEffectNew;

namespace Content.Shared._starcup.StatusEffectsNew;

public sealed partial class ComponentStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ComponentStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<ComponentStatusEffectComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
    }

    /// <summary>
    /// Gives the entity the component if they don't already have it.
    /// </summary>
    private void OnEffectApplied(Entity<ComponentStatusEffectComponent> entity, ref StatusEffectAppliedEvent args)
    {
        var component = _componentFactory.GetComponent(entity.Comp.Component);
        if (!HasComp(args.Target, component.GetType()))
        {
            AddComp(args.Target, component);
        }
    }

    /// <summary>
    /// Handles removing the component.
    /// </summary>
    private void OnEffectRemoved(Entity<ComponentStatusEffectComponent> entity, ref StatusEffectRemovedEvent args)
    {
        var component = _componentFactory.GetComponent(entity.Comp.Component);
        RemComp(args.Target, component.GetType());
    }
}
