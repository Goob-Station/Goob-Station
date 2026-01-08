// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2025 kurokoTurbo <92106367+kurokoTurbo@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client._Shitmed.Medical.Surgery.Wounds;

public sealed class WoundableVisualsSystem : VisualizerSystem<WoundableVisualsComponent>
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private const float AltBleedingSpriteChance = 0.15f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WoundableVisualsComponent, ComponentInit>(InitializeEntity, after: [typeof(WoundSystem)]);
        SubscribeLocalEvent<WoundableVisualsComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartRemovedEvent>(WoundableRemoved);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartAddedEvent>(WoundableConnected);
        SubscribeLocalEvent<WoundableVisualsComponent, WoundableIntegrityChangedEvent>(OnWoundableIntegrityChanged);
    }

    private void InitializeEntity(Entity<WoundableVisualsComponent> ent, ref ComponentInit args)
    {
        if (!TryComp(ent, out SpriteComponent? partSprite))
            return;

        InitDamage(ent, partSprite);
        InitBleeding(ent, partSprite);
    }

    private void InitBleeding(Entity<WoundableVisualsComponent> ent, SpriteComponent partSprite)
    {
        if (ent.Comp.BleedingOverlay == null)
            return;
        AddDamageLayerToSprite((ent, partSprite),
            ent.Comp.BleedingOverlay,
            $"{ent.Comp.OccupiedLayer}_Minor",
            $"{ent.Comp.OccupiedLayer}Bleeding");
    }

    private void InitDamage(Entity<WoundableVisualsComponent> ent, SpriteComponent partSprite)
    {
        if (ent.Comp.DamageOverlayGroups is null)
            return;
        foreach (var (group, sprite) in ent.Comp.DamageOverlayGroups)
            AddDamageLayerToSprite((ent, partSprite),
                sprite.Sprite,
                $"{ent.Comp.OccupiedLayer}_{group}_100",
                $"{ent.Comp.OccupiedLayer}{group}",
                sprite.Color);
    }

    private void OnAfterAutoHandleState(Entity<WoundableVisualsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out SpriteComponent? partSprite))
            return;

        UpdateWoundableVisuals(ent, (ent, partSprite));
    }

    private void WoundableConnected(Entity<WoundableVisualsComponent> ent, ref BodyPartAddedEvent args)
    {
        var bodyPart = args.Part.Comp;
        if (bodyPart.Body is not { } bodyUid ||
            !TryComp(bodyUid, out SpriteComponent? bodySprite) ||
            !HasComp<HumanoidAppearanceComponent>(bodyUid))
            return;

        if (ent.Comp.DamageOverlayGroups != null)
        {
            foreach (var (group, sprite) in ent.Comp.DamageOverlayGroups)
                if (!_sprite.LayerMapTryGet((bodyUid, bodySprite), $"{ent.Comp.OccupiedLayer}{group}", out _, false))
                {
                    AddDamageLayerToSprite((bodyUid, bodySprite),
                        sprite.Sprite,
                        $"{ent.Comp.OccupiedLayer}_{group}_100",
                        $"{ent.Comp.OccupiedLayer}{group}",
                        sprite.Color);
                }
        }

        if (ent.Comp.BleedingOverlay == null)
            return;
        if (!_sprite.LayerMapTryGet((bodyUid, bodySprite),
                $"{ent.Comp.OccupiedLayer}Bleeding",
                out _,
                false)
            && ent.Comp.BleedingOverlay != null)
        {
            AddDamageLayerToSprite((bodyUid, bodySprite),
                ent.Comp.BleedingOverlay,
                $"{ent.Comp.OccupiedLayer}_Minor",
                $"{ent.Comp.OccupiedLayer}Bleeding");
        }

        UpdateWoundableVisuals(ent, (bodyUid,bodySprite));
    }

    private void WoundableRemoved(Entity<WoundableVisualsComponent> ent, ref BodyPartRemovedEvent args)
    {
        var body = args.Part.Comp.Body;
        if (!TryComp(body, out SpriteComponent? bodySprite))
            return;

        foreach (var part in _body.GetBodyPartChildren(ent))
        {
            if (!TryComp<WoundableVisualsComponent>(part.Id, out var woundableVisuals))
                continue;
            RemoveWoundableLayers((body.Value, bodySprite), woundableVisuals);
            if (TryComp(ent, out SpriteComponent? pieceSprite))
                UpdateWoundableVisuals((part.Id, woundableVisuals), (ent, pieceSprite));
        }
    }

    private void RemoveWoundableLayers(Entity<SpriteComponent?> entity, WoundableVisualsComponent visuals)
    {
        if (visuals.DamageOverlayGroups == null)
            return;

        // Remove damage overlay layers
        foreach (var (group, _) in visuals.DamageOverlayGroups)
        {
            var layerKey = $"{visuals.OccupiedLayer}{group}";
            if (!_sprite.LayerMapTryGet(entity, layerKey, out var layer, false))
                continue;
            _sprite.LayerSetVisible(entity, layer, false);
            _sprite.RemoveLayer(entity, layer);
            _sprite.LayerMapRemove(entity, layerKey);
        }

        // Remove bleeding layer
        var bleedingKey = $"{visuals.OccupiedLayer}Bleeding";
        if (!_sprite.LayerMapTryGet(entity, bleedingKey, out var bleedLayer, false))
            return;
        _sprite.LayerSetVisible(entity, bleedLayer, false);
        _sprite.RemoveLayer(entity, bleedLayer, out _, false);
        _sprite.LayerMapRemove(entity, bleedingKey, out _);
    }

    private void OnWoundableIntegrityChanged(Entity<WoundableVisualsComponent> ent,
        ref WoundableIntegrityChangedEvent args)
    {
        var bodyPart = Comp<BodyPartComponent>(ent);
        if (!bodyPart.Body.HasValue)
        {
            if (TryComp(ent, out SpriteComponent? partSprite))
                UpdateWoundableVisuals(ent, (ent, partSprite));

            return;
        }

        if (TryComp(bodyPart.Body, out SpriteComponent? bodySprite))
            UpdateWoundableVisuals(ent, (bodyPart.Body.Value, bodySprite));
    }

    private void AddDamageLayerToSprite(Entity<SpriteComponent?> ent,
        string sprite,
        string state,
        string mapKey,
        string? color = null)
    {
        if (_sprite.LayerMapTryGet(ent, mapKey, out _, false)) // prevent dupes
            return;

        var newLayer = _sprite.AddLayer(ent,
            new SpriteSpecifier.Rsi(
                new ResPath(sprite),
                state
            ));
        _sprite.LayerMapSet(ent, mapKey, newLayer);
        if (color != null)
            _sprite.LayerSetColor(ent, newLayer, Color.FromHex(color));
        _sprite.LayerSetVisible(ent, newLayer, false);
    }

    private void UpdateWoundableVisuals(Entity<WoundableVisualsComponent> visuals, Entity<SpriteComponent?> sprite)
    {
        if (sprite.Comp is not { } spriteComponent)
            return;

        UpdateDamage(visuals, sprite);
        UpdateBleeding(visuals, visuals.Comp.OccupiedLayer, spriteComponent);
    }

    private void UpdateDamage(Entity<WoundableVisualsComponent> visuals, Entity<SpriteComponent?> sprite)
    {
        if (visuals.Comp.DamageOverlayGroups == null)
            return;
        foreach (var group in visuals.Comp.DamageOverlayGroups)
        {
            if (!_sprite.LayerMapTryGet(sprite, $"{visuals.Comp.OccupiedLayer}{group.Key}", out var damageLayer, false))
                continue;
            var severityPoint = _wound.GetWoundableSeverityPoint(visuals, damageGroup: group.Key);
            UpdateDamageLayerState(sprite,
                damageLayer,
                $"{visuals.Comp.OccupiedLayer}_{group.Key}",
                severityPoint <= visuals.Comp.Thresholds.FirstOrDefault() ? 0 : GetThreshold(severityPoint, visuals));
        }
    }

    private void UpdateBleeding(Entity<WoundableVisualsComponent> ent, Enum layer, SpriteComponent sprite)
    {
        if (!TryComp<BodyPartComponent>(ent, out var bodyPart))
            return;

        if (ent.Comp.BleedingOverlay == null)
        {
            if (!_body.TryGetParentBodyPart(ent, out var parentUid, out _))
                return;

            if (!_appearance.TryGetData<WoundVisualizerGroupData>(ent, WoundableVisualizerKeys.Wounds, out var wounds)
                || !_appearance.TryGetData<WoundVisualizerGroupData>(parentUid.Value,
                    WoundableVisualizerKeys.Wounds,
                    out var parentWounds))
                return;

            var woundList = new List<EntityUid>();
            woundList.AddRange(wounds.GroupList.Select(GetEntity));
            woundList.AddRange(parentWounds.GroupList.Select(GetEntity));

            var totalBleeds = (FixedPoint2) 0;
            foreach (var wound in woundList)
                if (TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    totalBleeds += bleeds.BleedingAmount;

            var symmetry = bodyPart.Symmetry == BodyPartSymmetry.Left ? "L" : "R";
            var partType = bodyPart.PartType == BodyPartType.Foot ? "Leg" : "Arm";

            var part = symmetry + partType;

            if (_sprite.LayerMapTryGet((ent, sprite), $"{part}Bleeding", out var parentBleedingLayer, false))
            {
                UpdateBleedingLayerState(
                    (ent, sprite),
                    parentBleedingLayer,
                    part,
                    totalBleeds,
                    GetBleedingThreshold(totalBleeds, ent));
            }
        }
        else
        {
            if (!_appearance.TryGetData<WoundVisualizerGroupData>(ent, WoundableVisualizerKeys.Wounds, out var wounds))
                return;

            var totalBleeds = (FixedPoint2) 0;
            foreach (var wound in wounds.GroupList.Select(GetEntity))
                if (TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    totalBleeds += bleeds.BleedingAmount;

            if (_sprite.LayerMapTryGet((ent, sprite), $"{layer}Bleeding", out var bleedingLayer, false))
            {
                UpdateBleedingLayerState((ent,sprite),
                    bleedingLayer,
                    layer.ToString(),
                    totalBleeds,
                    GetBleedingThreshold(totalBleeds, ent));
            }
        }
    }

    private static FixedPoint2 GetThreshold(FixedPoint2 threshold, WoundableVisualsComponent comp)
    {
        var nearestSeverity = FixedPoint2.Zero;

        foreach (var value in comp.Thresholds.OrderByDescending(kv => kv.Value))
        {
            if (threshold < value)
                continue;

            nearestSeverity = value;
            break;
        }

        return nearestSeverity;
    }

    private static BleedingSeverity GetBleedingThreshold(FixedPoint2 threshold, WoundableVisualsComponent comp)
    {
        var nearestSeverity = BleedingSeverity.Minor;

        foreach (var (key, value) in comp.BleedingThresholds.OrderByDescending(kv => kv.Value))
        {
            if (threshold < value)
                continue;

            nearestSeverity = key;
            break;
        }

        return nearestSeverity;
    }

    private void UpdateBleedingLayerState(Entity<SpriteComponent?> sprite,
        int spriteLayer,
        string statePrefix,
        FixedPoint2 damage,
        BleedingSeverity threshold)
    {
        if (damage <= 0)
        {
            _sprite.LayerSetVisible(sprite, spriteLayer, false);
            return;
        }

        if (!_sprite.TryGetLayer(sprite, spriteLayer, out var layer, false) || !layer.Visible)
            _sprite.LayerSetVisible(sprite, spriteLayer, true);

        var rsi = _sprite.LayerGetEffectiveRsi(sprite, spriteLayer);
        if (rsi == null)
            return;
        var state = $"{statePrefix}_{threshold}";
        var altState = $"{state}_alt";

        if (_random.Prob(AltBleedingSpriteChance) && rsi.TryGetState(altState, out _))
            _sprite.LayerSetRsiState(sprite, spriteLayer, altState);
        else if (rsi.TryGetState(state, out _))
            _sprite.LayerSetRsiState(sprite, spriteLayer, state);
    }

    private void UpdateDamageLayerState(Entity<SpriteComponent?> sprite,
        int spriteLayer,
        string statePrefix,
        FixedPoint2 threshold)
    {
        if (threshold <= 0)
            _sprite.LayerSetVisible(sprite, spriteLayer, false);
        else
        {
            if (!_sprite.TryGetLayer(sprite, spriteLayer, out var layer, false) || !layer.Visible)
                _sprite.LayerSetVisible(sprite, spriteLayer, true);
            _sprite.LayerSetRsiState(sprite, spriteLayer, $"{statePrefix}_{threshold}");
        }
    }
}
