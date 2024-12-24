using Content.Client.Overlays;
using Content.Shared.GameTicking;
using Content.Shared.NightVision.Components;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.NightVision;

public sealed class NightVisionSystem : EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;


    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new(Color.Green);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var comp in component.Components)
        {
            _overlay.NightvisionColor = comp.NightVisionColor;
            if (comp.IsNightVision)
                _lightManager.DrawLighting = false;
        }
        if (!_overlayMan.HasOverlay<NightVisionOverlay>())
        {
            _overlayMan.AddOverlay(_overlay);
        }
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }
}
