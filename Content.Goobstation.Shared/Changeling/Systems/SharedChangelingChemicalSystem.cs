using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.InternalResources.Components;
using Content.Goobstation.Shared.InternalResources.EntitySystems;
using Content.Goobstation.Shared.InternalResources.Events;
using Content.Shared.Atmos.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract partial class SharedChangelingChemicalSystem : EntitySystem
{
    [Dependency] private readonly SharedInternalResourcesSystem _resource = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;
    private EntityQuery<InternalResourcesComponent> _resourceQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingChemicalComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChangelingChemicalComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<ChangelingChemicalComponent, InternalResourcesRegenModifierEvent>(BeforeResourceRegenEvent);
        SubscribeLocalEvent<ChangelingChemicalComponent, RejuvenateEvent>(OnRejuvenate);

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();
        _resourceQuery = GetEntityQuery<InternalResourcesComponent>();
    }

    private void OnMapInit(Entity<ChangelingChemicalComponent> ent, ref MapInitEvent args)
    {
        _resource.TryAddInternalResources(ent, ent.Comp.ResourceProto, out var data);

        if (data != null)
            ent.Comp.ResourceData = data;
    }

    private void OnShutdown(Entity<ChangelingChemicalComponent> ent, ref ComponentShutdown args)
    {
        if (_resourceQuery.TryComp(ent, out var resComp))
            _resource.TryRemoveInternalResource(ent, ent.Comp.ResourceProto, resComp);
    }

    #region Event Handlers

    private void BeforeResourceRegenEvent(Entity<ChangelingChemicalComponent> ent, ref InternalResourcesRegenModifierEvent args)
    {
        if (ent.Comp.ResourceData == null
            || args.Data.InternalResourcesType != ent.Comp.ResourceData.InternalResourcesType)
            return;

        if (OnFire(ent))
            args.Modifier *= ent.Comp.FireModifier;
    }

    private void OnRejuvenate(Entity<ChangelingChemicalComponent> ent, ref RejuvenateEvent args)
    {
        if (ent.Comp.ResourceData == null
            || _lingQuery.TryComp(ent, out var ling)
            && ling.IsInStasis)
            return;

        _resource.TryUpdateResourcesAmount(ent, ent.Comp.ResourceData, ent.Comp.ResourceData.MaxAmount);

        _popup.PopupEntity(Loc.GetString(ent.Comp.RejuvenatePopup), ent, ent);
    }

    #endregion

    #region Helper Methods

    private bool OnFire(Entity<ChangelingChemicalComponent> ent)
    {
        return HasComp<OnFireComponent>(ent);
    }

    #endregion
}
