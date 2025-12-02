// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Overlays;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Overlays;

public sealed class NightVisionSystem : EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private NightVisionOverlay _nvgOverlay = default!;
    private BaseSwitchableOverlay<NightVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _nvgOverlay = new NightVisionOverlay();
        _overlay = new BaseSwitchableOverlay<NightVisionComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<NightVisionComponent> ent,
        ref RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        if (!ent.Comp.IsEquipment)
            base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<NightVisionComponent> ent,
        ref InventoryRelayedEvent<RefreshEquipmentHudEvent<NightVisionComponent>> args)
    {
        if (ent.Comp.IsEquipment)
            base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<NightVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.UpdateInternal(args);
        NightVisionComponent? nvComp = null;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (nvComp == null)
                nvComp = comp;
            else if (!nvComp.DrawOverlay && comp.DrawOverlay)
                nvComp = comp;
            else if (nvComp.DrawOverlay == comp.DrawOverlay && nvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                nvComp = comp;

            lightRadius = MathF.Max(lightRadius, comp.LightRadius);
        }

        UpdateNightVision(nvComp, lightRadius);
        UpdateOverlay(nvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _nvgOverlay.ResetLight(false);
        UpdateOverlay(null);
        UpdateNightVision(null, 0f);
    }

    private void UpdateNightVision(NightVisionComponent? comp, float lightRadius)
    {
        _nvgOverlay.LightRadius = lightRadius;
        _nvgOverlay.Comp = comp;

        switch (comp)
        {
            case not null when !_overlayMan.HasOverlay<NightVisionOverlay>():
                _overlayMan.AddOverlay(_nvgOverlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_nvgOverlay);
                _nvgOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(NightVisionComponent? nvComp)
    {
        _overlay.Comp = nvComp;

        switch (nvComp)
        {
            case { DrawOverlay: true } when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null or { DrawOverlay: false }:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        if (_overlayMan.TryGetOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>(out var overlay))
            overlay.IsActive = nvComp == null;
    }
}
