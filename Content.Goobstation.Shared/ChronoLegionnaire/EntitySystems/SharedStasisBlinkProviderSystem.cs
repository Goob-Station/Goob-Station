using Content.Goobstation.Shared.ChronoLegionnaire.Components;
using Content.Shared.Actions;

namespace Content.Goobstation.Shared.ChronoLegionnaire.EntitySystems;

public abstract class SharedStasisBlinkProviderSystem : EntitySystem
{
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisBlinkProviderComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<StasisBlinkProviderComponent, GetItemActionsEvent>(OnGetItemActions);
    }

    private void OnMapInit(Entity<StasisBlinkProviderComponent> provider, ref MapInitEvent args)
    {
        var comp = provider.Comp;

        _actionContainer.EnsureAction(provider, ref comp.BlinkActionEntity, comp.BlinkAction);

        Dirty(provider, comp);
    }

    private void OnGetItemActions(Entity<StasisBlinkProviderComponent> provider, ref GetItemActionsEvent args)
    {
        var comp = provider.Comp;

        args.AddAction(ref comp.BlinkActionEntity, comp.BlinkAction);
    }
}
