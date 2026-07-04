// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.CosmicCult;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Inventory.Events;
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
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private readonly ResPath _rsiPath = new("/Textures/_DV/CosmicCult/Effects/ability_siphonvfx.rsi");

    private readonly SoundSpecifier _siphonSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_siphon.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicSubtleMarkComponent, DidEquipEvent>((uid, _, _) => UpdateSubtleMarkVisibility(uid));
        SubscribeLocalEvent<CosmicSubtleMarkComponent, DidEquipHandEvent>((uid, _, _) => UpdateSubtleMarkVisibility(uid));
        SubscribeLocalEvent<CosmicSubtleMarkComponent, DidUnequipEvent>((uid, _, _) => UpdateSubtleMarkVisibility(uid));
        SubscribeLocalEvent<CosmicSubtleMarkComponent, DidUnequipHandEvent>((uid, _, _) => UpdateSubtleMarkVisibility(uid));
        SubscribeLocalEvent<CosmicSubtleMarkComponent, WearerMaskToggledEvent>((uid, _, _) => UpdateSubtleMarkVisibility(uid));

        SubscribeLocalEvent<CosmicStarMarkComponent, ComponentStartup>(OnCosmicStarMarkAdded);
        SubscribeLocalEvent<CosmicStarMarkComponent, ComponentShutdown>(OnCosmicStarMarkRemoved);

        SubscribeLocalEvent<CosmicSubtleMarkComponent, ComponentStartup>(OnCosmicSubtleMarkAdded);
        SubscribeLocalEvent<CosmicSubtleMarkComponent, ComponentShutdown>(OnCosmicSubtleMarkRemoved);

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
        var layer = _sprite.AddLayer((ent, sprite), new SpriteSpecifier.Rsi(_rsiPath, "vfx"));
        _sprite.LayerMapSet((ent, sprite), CultSiphonedVisuals.Key, layer);
        _sprite.LayerSetOffset((ent, sprite), layer, new Vector2(0, 0.8f));
        _sprite.LayerSetScale((ent, sprite), layer, new Vector2(0.65f, 0.65f));
        sprite.LayerSetShader(layer, "unshaded");

        Timer.Spawn(TimeSpan.FromSeconds(2), () => _sprite.RemoveLayer((ent, sprite), CultSiphonedVisuals.Key));
        _audio.PlayLocal(_siphonSFX, ent, ent, AudioParams.Default.WithVariation(0.1f));
    }

    private void OnUpdateAlert(Entity<CosmicCultComponent> ent, ref UpdateAlertSpriteEvent args)
    {
        if (args.Alert.ID != ent.Comp.EntropyAlert)
            return;
        var entropy = Math.Clamp(ent.Comp.EntropyStored, 0, 14);
        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), AlertVisualLayers.Base, $"base{entropy}");
        _sprite.LayerSetRsiState(args.SpriteViewEnt.AsNullable(), CultAlertVisualLayers.Counter, $"num{entropy}");
    }
    #endregion

    #region Layer Additions
    private void OnCosmicStarMarkAdded(Entity<CosmicStarMarkComponent> uid, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || _sprite.LayerMapTryGet((uid, sprite), CosmicRevealedKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), uid.Comp.Sprite);
        _sprite.LayerMapSet((uid, sprite), CosmicRevealedKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");

        //offset the mark if the mob has an offset comp, needed for taller species like Thaven
        if (TryComp<CosmicMarkVisualsComponent>(uid, out var offset))
        {
            _sprite.LayerSetOffset((uid, sprite), CosmicRevealedKey.Key, offset.Offset);
            _sprite.LayerSetRsiState((uid, sprite), layer, offset.StarState);
        }
    }

    private void OnCosmicSubtleMarkAdded(Entity<CosmicSubtleMarkComponent> uid, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || _sprite.LayerMapTryGet((uid, sprite), CosmicRevealedKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), uid.Comp.Sprite);
        _sprite.LayerMapSet((uid, sprite), CosmicRevealedKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");

        UpdateSubtleMarkVisibility(uid);

        //I am NOT accounting for IPCs here. If you want it, do it yourself. You guys wanted them to be able to look like any other species, not me.
        //Also there's probably a better solution but meh, this works.
        if (TryComp<CosmicMarkVisualsComponent>(uid, out var offset))
        {
            _sprite.LayerSetOffset((uid, sprite), CosmicRevealedKey.Key, offset.Offset);
            _sprite.LayerSetRsiState((uid, sprite), layer, offset.SubtleState);
        }
    }

    private void OnCosmicImpositionAdded(Entity<CosmicImposingComponent> uid, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || _sprite.LayerMapTryGet((uid, sprite), CosmicImposingKey.Key, out _, false))
            return;

        var layer = _sprite.AddLayer((uid, sprite), uid.Comp.Sprite);

        _sprite.LayerMapSet((uid, sprite), CosmicImposingKey.Key, layer);
        sprite.LayerSetShader(layer, "unshaded");
    }
    #endregion

    #region Layer Removals
    private void OnCosmicStarMarkRemoved(Entity<CosmicStarMarkComponent> uid, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.RemoveLayer((uid, sprite), CosmicRevealedKey.Key);
    }

    private void OnCosmicSubtleMarkRemoved(Entity<CosmicSubtleMarkComponent> uid, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.RemoveLayer((uid, sprite), CosmicRevealedKey.Key);
    }

    private void OnCosmicImpositionRemoved(Entity<CosmicImposingComponent> uid, ref ComponentShutdown args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        _sprite.RemoveLayer((uid, sprite), CosmicImposingKey.Key);
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

    #region Mark updates
    private void UpdateSubtleMarkVisibility(EntityUid uid)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite) || !_sprite.LayerMapTryGet((uid, sprite), CosmicRevealedKey.Key, out var layer, false))
            return;

        if (!TryComp<CosmicSubtleMarkComponent>(uid, out var markComp))
            return;

        var ev = new SeeIdentityAttemptEvent();
        RaiseLocalEvent(uid, ev);
        var eyesCovered = ev.TotalCoverage.HasFlag(IdentityBlockerCoverage.EYES);
        _sprite.LayerSetVisible((uid, sprite), layer, !eyesCovered);
    }
    #endregion
}

public enum CultSiphonedVisuals : byte
{
    Key
}
