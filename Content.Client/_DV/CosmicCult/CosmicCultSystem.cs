// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;
using Content.Shared._DV.CosmicCult.Components.Examine;
using System.Numerics;
using Robust.Client.Audio;
using Robust.Shared.Audio;
using Content.Client.Alerts;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Timer = Robust.Shared.Timing.Timer;

namespace Content.Client._DV.CosmicCult;

public sealed partial class CosmicCultSystem : SharedCosmicCultSystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly ResPath _rsiPath = new("/Textures/_DV/CosmicCult/Effects/ability_siphonvfx.rsi");

    private readonly SoundSpecifier _siphonSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_siphon.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicStarMarkComponent, ComponentStartup>(OnCosmicStarMarkAdded);
        SubscribeLocalEvent<CosmicStarMarkComponent, ComponentShutdown>(OnCosmicStarMarkRemoved);

        SubscribeLocalEvent<CosmicImposingComponent, ComponentStartup>(OnCosmicImpositionAdded);
        SubscribeLocalEvent<CosmicImposingComponent, ComponentShutdown>(OnCosmicImpositionRemoved);

        SubscribeLocalEvent<CosmicCultComponent, GetStatusIconsEvent>(GetCosmicCultIcon);
        SubscribeLocalEvent<CosmicCultLeadComponent, GetStatusIconsEvent>(GetCosmicCultLeadIcon);
        SubscribeLocalEvent<CosmicBlankComponent, GetStatusIconsEvent>(GetCosmicSSDIcon);

        SubscribeNetworkEvent<CosmicSiphonIndicatorEvent>(OnSiphon);
        SubscribeLocalEvent<CosmicCultComponent, UpdateAlertSpriteEvent>(OnUpdateAlert);
    }

    #region Siphon Visuals
    private void OnSiphon(CosmicSiphonIndicatorEvent args)
    {
        var ent = GetEntity(args.Target);
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;
        var layer = sprite.AddLayer(new SpriteSpecifier.Rsi(_rsiPath, "vfx"));
        sprite.LayerMapSet(CultSiphonedVisuals.Key, layer);
        sprite.LayerSetOffset(layer, new Vector2(0, 0.8f));
        sprite.LayerSetScale(layer, new Vector2(0.65f, 0.65f));
        sprite.LayerSetShader(layer, "unshaded");

        Timer.Spawn(TimeSpan.FromSeconds(2), () => sprite.RemoveLayer(CultSiphonedVisuals.Key));
        _audio.PlayLocal(_siphonSFX, ent, ent, AudioParams.Default.WithVariation(0.1f));
    }

    private void OnUpdateAlert(Entity<CosmicCultComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.EntropyAlert)
            return;
        var entropy = Math.Clamp(ent.Comp.EntropyStored, 0, 14);
        var sprite = args.SpriteViewEnt.Comp;
        sprite.LayerSetState(AlertVisualLayers.Base, $"base{entropy}");
        sprite.LayerSetState(CultAlertVisualLayers.Counter, $"num{entropy}");
    }
    #endregion

    #region Layer Additions
    private void OnCosmicStarMarkAdded(Entity<CosmicStarMarkComponent> uid, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || sprite.LayerMapTryGet(CosmicRevealedKey.Key, out _))
            return;

        var layer = sprite.AddLayer(uid.Comp.Sprite);
        sprite.LayerMapSet(CosmicRevealedKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");

        //offset the mark if the mob has an offset comp, needed for taller species like Thaven
        if (TryComp<CosmicStarMarkOffsetComponent>(uid, out var offset))
        {
            sprite.LayerSetOffset(CosmicRevealedKey.Key, offset.Offset);
        }
    }

    private void OnCosmicImpositionAdded(Entity<CosmicImposingComponent> uid, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || sprite.LayerMapTryGet(CosmicImposingKey.Key, out _))
            return;

        var layer = sprite.AddLayer(uid.Comp.Sprite);

        sprite.LayerMapSet(CosmicImposingKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }
    #endregion

    #region Layer Removals
    private void OnCosmicStarMarkRemoved(Entity<CosmicStarMarkComponent> uid, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.RemoveLayer(CosmicRevealedKey.Key);
    }

    private void OnCosmicImpositionRemoved(Entity<CosmicImposingComponent> uid, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        sprite.RemoveLayer(CosmicImposingKey.Key);
    }
    #endregion

    #region Icons
    private void GetCosmicCultIcon(Entity<CosmicCultComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<CosmicCultLeadComponent>(ent))
            return;

        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetCosmicCultLeadIcon(Entity<CosmicCultLeadComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void GetCosmicSSDIcon(Entity<CosmicBlankComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
    #endregion
}

public enum CultSiphonedVisuals : byte
{
    Key
}
