using Content.Client.Overlays;
using Content.Shared._White.Overlays;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Client._White.Overlays;

public sealed class ThermalVisionSystem : EquipmentHudSystem<ThermalVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private ThermalVisionOverlay _thermalOverlay = default!;
    private BaseSwitchableOverlay<ThermalVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThermalVisionComponent, SwitchableOverlayToggledEvent>(OnToggle, after: new[] { typeof(NightVisionSystem) });

        _thermalOverlay = new ThermalVisionOverlay();
        _overlay = new BaseSwitchableOverlay<ThermalVisionComponent>();
    }

    private void OnToggle(Entity<ThermalVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay(args.Performer);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<ThermalVisionComponent> args)
    {
        base.UpdateInternal(args);
        ThermalVisionComponent? tvComp = null;
        var active = false;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (comp.IsActive)
                active = true;
            else
                continue;

            if (comp.DrawOverlay)
                tvComp ??= comp;

            lightRadius = MathF.Max(lightRadius, comp.LightRadius);
        }

        UpdateThermalOverlay(active, lightRadius);
        UpdateOverlay(tvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        UpdateOverlay(null);
        UpdateThermalOverlay(false, 0f);
    }

    private void UpdateThermalOverlay(bool active, float lightRadius)
    {
        switch (active)
        {
            case true when !_overlayMan.HasOverlay<ThermalVisionOverlay>():
                _thermalOverlay.LightRadius = lightRadius;
                _overlayMan.AddOverlay(_thermalOverlay);
                break;
            case false:
                _overlayMan.RemoveOverlay(_thermalOverlay);
                _thermalOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(ThermalVisionComponent? tvComp)
    {
        switch (tvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>():
                _overlay.Comp = tvComp;
                _overlayMan.AddOverlay(_overlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        // Night vision overlay is prioritized
        _overlay.IsActive = !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>();
    }
}
