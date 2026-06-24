// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Medical.Common.Vomiting;
using Content.Shared.Damage.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Systems;
using Content.Trauma.Shared.AnimalAgeing;
using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Systems;

public sealed partial class SharedHappinessSystem : EntitySystem
{
    [Dependency] private SharedInternalResourcesSystem _internalResources = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedAnimalAgeingSystem _ageing = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    private EntityQuery<InternalResourcesComponent> _internalResourcesQuery;

    public override void Initialize()
    {
        base.Initialize();

        _internalResourcesQuery = GetEntityQuery<InternalResourcesComponent>();

        SubscribeLocalEvent<HappinessComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HappinessComponent, InteractionSuccessEvent>(OnSuccessPet);
        SubscribeLocalEvent<HappinessComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<HappinessComponent, VomitedEvent>(OnVomit);

        SubscribeLocalEvent<AddComponentOnHappyComponent, InternalResourcesAmountChangedEvent>(OnHappinessChanged);
        SubscribeLocalEvent<ReplaceOnUnhappyComponent, InternalResourcesAmountChangedEvent>(OnHappinessChangedReplace);
    }

    private void OnVomit(Entity<HappinessComponent> ent, ref VomitedEvent args)
    {
        ChangeHappiness(ent, ent.Comp.DamageDecrease);
    }

    private void OnDamaged(Entity<HappinessComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.Origin is null || args.Origin == ent.Owner)
            return;

        ChangeHappiness(ent, ent.Comp.DamageDecrease);
    }

    private void OnSuccessPet(Entity<HappinessComponent> ent, ref InteractionSuccessEvent args)
    {
        ChangeHappiness(ent, ent.Comp.HappinessIncrease);
    }

    private void OnHappinessChangedReplace(Entity<ReplaceOnUnhappyComponent> ent, ref InternalResourcesAmountChangedEvent args)
    {
        if (args.NewAmount <= ent.Comp.HappinessRequired)
            _ageing.CopyAndReplaceEntity(ent.Comp.Ent, ent.Owner);
    }

    private void OnHappinessChanged(Entity<AddComponentOnHappyComponent> ent, ref InternalResourcesAmountChangedEvent args)
    {
        if (!TryComp<HappinessComponent>(ent.Owner, out var happiness))
            return;

        var enthappiness = GetHappiness((ent.Owner, happiness));

        if (enthappiness is null || enthappiness < ent.Comp.HappinessRequired)
            return;

        EntityManager.AddComponents(ent.Owner, ent.Comp.Components);
    }

    private void OnMapInit(Entity<HappinessComponent> ent, ref MapInitEvent args)
    {
        var happinessResource = _proto.Index(ent.Comp.HappinessResource);
        _internalResources.EnsureInternalResources(ent.Owner, happinessResource, out _);
    }

    public void SetHappiness(Entity<HappinessComponent> ent, float setTo)
    {
        if (!TryComp<InternalResourcesComponent>(ent, out var internalResources))
            return;

        var happinessResource = _proto.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                type.CurrentAmount = setTo;
        }
    }

    public void ChangeHappiness(Entity<HappinessComponent> ent, float amount)
    {
        if (!_internalResourcesQuery.TryComp(ent, out var internalResources) || _mobState.IsDead(ent.Owner))
            return;

        var happinessResource = _proto.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                type.CurrentAmount += amount;
        }
    }

    public float? GetHappiness(Entity<HappinessComponent> ent)
    {
        if (!TryComp<InternalResourcesComponent>(ent, out var internalResources))
            return null;

        var happinessResource = _proto.Index(ent.Comp.HappinessResource);

        foreach (var type in internalResources.CurrentInternalResources)
        {
            if (type.InternalResourcesType == happinessResource)
                return type.CurrentAmount;
        }

        return null;
    }
}
