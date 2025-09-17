// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 DoutorWhite <thedoctorwhite@gmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SX-7 <sn1.test.preria.2002@gmail.com>
// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.CCVar;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;

namespace Content.Client.Light.EntitySystems;

public sealed class PlanetLightSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfgManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    /// <summary>
    /// Enables / disables the ambient occlusion overlay.
    /// </summary>
    public bool AmbientOcclusion
    {
        get => _ambientOcclusion;
        set
        {
            if (_ambientOcclusion == value)
                return;

            _ambientOcclusion = value;

            if (value)
            {
                _overlayMan.AddOverlay(new AmbientOcclusionOverlay());
            }
            else
            {
                _overlayMan.RemoveOverlay<AmbientOcclusionOverlay>();
            }
        }
    }

    private bool _ambientOcclusion;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GetClearColorEvent>(OnClearColor);

        _cfgManager.OnValueChanged(CCVars.AmbientOcclusion, val =>
        {
            AmbientOcclusion = val;
        }, true);

        _overlayMan.AddOverlay(new BeforeLightTargetOverlay());
        _overlayMan.AddOverlay(new RoofOverlay(EntityManager));
        _overlayMan.AddOverlay(new TileEmissionOverlay(EntityManager));
        _overlayMan.AddOverlay(new LightBlurOverlay());
        _overlayMan.AddOverlay(new SunShadowOverlay());
        _overlayMan.AddOverlay(new AfterLightTargetOverlay());
    }

    private void OnClearColor(ref GetClearColorEvent ev)
    {
        ev.Color = Color.Transparent;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<BeforeLightTargetOverlay>();
        _overlayMan.RemoveOverlay<RoofOverlay>();
        _overlayMan.RemoveOverlay<TileEmissionOverlay>();
        _overlayMan.RemoveOverlay<LightBlurOverlay>();
        _overlayMan.RemoveOverlay<SunShadowOverlay>();
        _overlayMan.RemoveOverlay<AfterLightTargetOverlay>();
    }
}