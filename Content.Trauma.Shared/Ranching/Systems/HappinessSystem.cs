using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Coordinates;
using Content.Shared.Interaction.Events;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.Ranching.Components;
using Robust.Shared.Prototypes;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed class HappinessSystem : EntitySystem
{
    [Dependency] private readonly SharedInternalResourcesSystem _internalResources = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedAnimalAgeingSystem _ageingSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HappinessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HappinessComponent, InteractionSuccessEvent>(OnSuccessPet);

        SubscribeLocalEvent<ReplaceOnHappyComponent, InternalResourcesAmountChangedEvent>(OnHappinessChanged);
    }

    private void OnHappinessChanged(Entity<ReplaceOnHappyComponent> ent, ref InternalResourcesAmountChangedEvent args)
    {
        if (!TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        var enthappiness = GetHappiness((ent.Owner, happiness));

        if (enthappiness is null || enthappiness < ent.Comp.HappinessRequired)
            return;

        _ageingSystem.CopyAndReplaceEntity(ent.Comp.Entity, ent.Owner);
    }

    private void OnSuccessPet(Entity<HappinessComponent> ent, ref InteractionSuccessEvent args)
    {
        if (!TryComp<InternalResourcesComponent>(ent, out var internalResources))
            return;

        var happinessResource = _prototype.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                _internalResources.TryUpdateResourcesAmount(ent.Owner, type, ent.Comp.HappinessIncrease);
        }
    }

    private void OnMapInit(Entity<HappinessComponent> ent, ref MapInitEvent args)
    {
        var happinessResource = _prototype.Index(ent.Comp.HappinessResource);
        _internalResources.EnsureInternalResources(ent.Owner, happinessResource, out _);
    }

    public float? GetHappiness(Entity<HappinessComponent> ent)
    {
        if (!TryComp<InternalResourcesComponent>(ent, out var internalResources))
            return null;

        var happinessResource = _prototype.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                return type.CurrentAmount;
        }

        return null;
    }
}
