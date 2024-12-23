using Content.Client.Overlays;
using Content.Shared._White.Overlays;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Client._White.Overlays;

public sealed class NightVisionSystem : EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;

    private BaseSwitchableOverlay<NightVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _overlay = new BaseSwitchableOverlay<NightVisionComponent>();
    }

    private void OnToggle(Entity<NightVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay(args.Performer);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.UpdateInternal(args);
        var active = false;
        NightVisionComponent? nvComp = null;
        foreach (var comp in args.Components)
        {
            if (comp.IsActive)
                active = true;
            else
                continue;

            if (comp.DrawOverlay)
                nvComp ??= comp;

            if (active && nvComp != null)
                break;
        }

        UpdateNightVision(active);
        UpdateOverlay(nvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        UpdateNightVision(false);
        UpdateOverlay(null);
    }

    private void UpdateNightVision(bool active)
    {
        _lightManager.DrawLighting = !active;
    }

    private void UpdateOverlay(NightVisionComponent? nvComp)
    {
        switch (nvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>():
                _overlay.Comp = nvComp;
                _overlayMan.AddOverlay(_overlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        if (_overlayMan.TryGetOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>(out var overlay))
            overlay.IsActive = nvComp == null;
    }
}
