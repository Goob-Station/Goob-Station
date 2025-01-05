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
        RefreshOverlay(args.User);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.UpdateInternal(args);

        var active = false;
        NightVisionComponent? nvComp = null;
        foreach (var comp in args.Components)
        {
            if (comp.IsActive || comp.PulseTime > 0f && comp.PulseAccumulator < comp.PulseTime)
                active = true;
            else
                continue;

            if (comp.DrawOverlay)
            {
                if (nvComp == null)
                    nvComp = comp;
                else if (nvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                    nvComp = comp;
            }

            if (active && nvComp is { PulseTime: <= 0 })
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
        _overlay.Comp = nvComp;

        switch (nvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>():
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
