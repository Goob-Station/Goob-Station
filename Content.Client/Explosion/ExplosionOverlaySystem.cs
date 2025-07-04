// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aexxie <codyfox.077@gmail.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Explosion;
using Content.Shared.Explosion.Components;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.GameStates;
using Robust.Shared.Graphics.RSI;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Explosion;

/// <summary>
///     This system is responsible for showing the client-side explosion effects (light source & fire-overlay). The
///     fire overlay code is just a bastardized version of the atmos plasma fire overlay and uses the same texture.
/// </summary>
public sealed class ExplosionOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IResourceCache _resCache = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedPointLightSystem _lights = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExplosionVisualsComponent, ComponentInit>(OnExplosionInit);
        SubscribeLocalEvent<ExplosionVisualsComponent, ComponentRemove>(OnCompRemove);
        SubscribeLocalEvent<ExplosionVisualsComponent, ComponentHandleState>(OnExplosionHandleState);
        _overlayMan.AddOverlay(new ExplosionOverlay(_appearance));
    }

    private void OnExplosionHandleState(EntityUid uid, ExplosionVisualsComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ExplosionVisualsState state)
            return;

        component.Epicenter = state.Epicenter;
        component.SpaceTiles = state.SpaceTiles;
        component.Tiles.Clear();

        foreach (var (nent, data) in state.Tiles)
        {
            component.Tiles[GetEntity(nent)] = data;
        }

        component.Intensity = state.Intensity;
        component.ExplosionType = state.ExplosionType;
        component.SpaceMatrix = state.SpaceMatrix;
        component.SpaceTileSize = state.SpaceTileSize;
    }

    private void OnCompRemove(EntityUid uid, ExplosionVisualsComponent component, ComponentRemove args)
    {
        if (TryComp(uid, out ExplosionVisualsTexturesComponent? textures) && !Deleted(textures.LightEntity))
            QueueDel(textures.LightEntity);
    }

    private void OnExplosionInit(EntityUid uid, ExplosionVisualsComponent component, ComponentInit args)
    {
        EnsureComp<ExplosionVisualsTexturesComponent>(uid);

        if (!_protoMan.TryIndex(component.ExplosionType, out ExplosionPrototype? type) ||
            !TryComp(uid, out ExplosionVisualsTexturesComponent? textures))
        {
            return;
        }

        // Map may have been deleted.
        if (_mapMan.MapExists(component.Epicenter.MapId))
        {
            // spawn in a client-side light source at the epicenter
            var lightEntity = Spawn("ExplosionLight", component.Epicenter);
            var light = _lights.EnsureLight(lightEntity);

            _lights.SetRadius(lightEntity, component.Intensity.Count, light);
            _lights.SetEnergy(lightEntity, component.Intensity.Count, light);
            _lights.SetColor(lightEntity, type.LightColor, light);
            textures.LightEntity = lightEntity;
        }


        textures.FireColor = type.FireColor;
        textures.IntensityPerState = type.IntensityPerState;

        var fireRsi = _resCache.GetResource<RSIResource>(type.TexturePath).RSI;
        foreach (var state in fireRsi)
        {
            textures.FireFrames.Add(state.GetFrames(RsiDirection.South));
            if (textures.FireFrames.Count == type.FireStates)
                break;
        }
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<ExplosionOverlay>();
    }
}