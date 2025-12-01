using Content.Client.Overlays;
using Content.Goobstation.Shared.Disease;
using Content.Shared.Inventory.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Client.Disease;

/// <summary>
/// Shows icons on infected mobs.
/// </summary>
public sealed class ShowDiseaseIconsSystem : EquipmentHudSystem<Shared.Disease.Components.ShowDiseaseIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private float? LowThreshold, MediumThreshold, HighThreshold;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Disease.Components.DiseaseCarrierComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
        SubscribeLocalEvent<Shared.Disease.Components.ShowDiseaseIconsComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<Shared.Disease.Components.ShowDiseaseIconsComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var comp in component.Components)
        {
            if (comp.LowThreshold != null)
                LowThreshold = MathF.Min(LowThreshold ?? int.MaxValue, comp.LowThreshold.Value);
            if (comp.MediumThreshold != null)
                MediumThreshold = MathF.Min(MediumThreshold ?? int.MaxValue, comp.MediumThreshold.Value);
            if (comp.HighThreshold != null)
                HighThreshold = MathF.Min(HighThreshold ?? int.MaxValue, comp.HighThreshold.Value);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        LowThreshold = null;
        MediumThreshold = null;
        HighThreshold = null;
    }

    private void OnHandleState(Entity<Shared.Disease.Components.ShowDiseaseIconsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        RefreshOverlay();
    }

    private void OnGetStatusIconsEvent(Entity<Shared.Disease.Components.DiseaseCarrierComponent> entity, ref GetStatusIconsEvent args)
    {
        if (!IsActive)
            return;

        var diseaseIcon = DecideDiseaseIcon(entity);

        if (diseaseIcon != null)
            args.StatusIcons.Add(diseaseIcon);
    }

    private DiseaseIconPrototype? DecideDiseaseIcon(Entity<Shared.Disease.Components.DiseaseCarrierComponent> entity)
    {
        var carrier = entity.Comp;
        var total = 0f;
        foreach (var disease in carrier.Diseases)
        {
            if (!TryComp<Shared.Disease.Components.DiseaseComponent>(disease, out var comp))
                continue;

            total += comp.InfectionProgress * comp.Complexity;
        }
        if (total > (HighThreshold ?? int.MaxValue) && _proto.TryIndex(carrier.HighIcon, out var highIcon))
            return highIcon;
        else if (total > (MediumThreshold ?? int.MaxValue) && _proto.TryIndex(carrier.MediumIcon, out var medIcon))
            return medIcon;
        else if (total > (LowThreshold ?? int.MaxValue) && _proto.TryIndex(carrier.LowIcon, out var lowIcon))
            return lowIcon;

        return null;
    }
}
