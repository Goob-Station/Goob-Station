// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Kayzel <43700376+KayzelW@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Roudenn <romabond091@gmail.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Trest <144359854+trest100@users.noreply.github.com>
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
    private const float AltBleedingSpriteChance = 0.15f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WoundableVisualsComponent, ComponentInit>(InitializeEntity, after: [typeof(WoundSystem)]);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartRemovedEvent>(WoundableRemoved);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartAddedEvent>(WoundableConnected);
        SubscribeLocalEvent<WoundableVisualsComponent, WoundableIntegrityChangedEvent>(OnWoundableIntegrityChanged);
    }

    private void InitializeEntity(EntityUid uid, WoundableVisualsComponent component, ref ComponentInit args)
    {
        if (!TryComp(uid, out SpriteComponent? partSprite))
            return;

        foreach (var (group, sprite) in component.DamageOverlayGroups!)
        {
            AddDamageLayerToSprite(partSprite,
                sprite.Sprite,
                $"{component.OccupiedLayer}_{group}_100",
                $"{component.OccupiedLayer}{group}",
                sprite.Color);
        }

        if (component.BleedingOverlay != null)
        {
            AddDamageLayerToSprite(partSprite,
                component.BleedingOverlay,
                $"{component.OccupiedLayer}_Minor",
                $"{component.OccupiedLayer}Bleeding");
        }
    }

    private void WoundableConnected(EntityUid uid, WoundableVisualsComponent component, ref BodyPartAddedEvent args)
    {
        var bodyPart = args.Part.Comp;
        if (!bodyPart.Body.HasValue
            || !TryComp(bodyPart.Body.Value, out SpriteComponent? bodySprite)
            || !HasComp<HumanoidAppearanceComponent>(bodyPart.Body.Value))
            return;

        foreach (var (group, sprite) in component.DamageOverlayGroups!)
        {
            if (!bodySprite.LayerMapTryGet($"{component.OccupiedLayer}{group}", out _))
            {
                AddDamageLayerToSprite(bodySprite,
                    sprite.Sprite,
                    $"{component.OccupiedLayer}_{group}_100",
                    $"{component.OccupiedLayer}{group}",
                    sprite.Color);
            }
        }

        if (!bodySprite.LayerMapTryGet($"{component.OccupiedLayer}Bleeding", out _) && component.BleedingOverlay != null)
        {
            AddDamageLayerToSprite(bodySprite,
                component.BleedingOverlay,
                $"{component.OccupiedLayer}_Minor",
                $"{component.OccupiedLayer}Bleeding");
        }
    }

    private void WoundableRemoved(EntityUid uid, WoundableVisualsComponent component, ref BodyPartRemovedEvent args)
    {
        var body = args.Part.Comp.Body;
        if (!TryComp(body, out SpriteComponent? bodySprite))
            return;

        foreach (var part in _body.GetBodyPartChildren(uid))
        {
            if (!TryComp<WoundableVisualsComponent>(part.Id, out var woundableVisuals))
                continue;

            foreach (var (group, _) in woundableVisuals.DamageOverlayGroups!)
            {
                if (!bodySprite.LayerMapTryGet($"{woundableVisuals.OccupiedLayer}{group}", out var layer))
                    continue;

                bodySprite.LayerSetVisible(layer, false);
                bodySprite.LayerMapRemove(layer);
            }

            if (bodySprite.LayerMapTryGet($"{woundableVisuals.OccupiedLayer}Bleeding", out var childBleeds))
            {
                bodySprite.LayerSetVisible(childBleeds, false);
                bodySprite.LayerMapRemove(childBleeds);
            }

            if (TryComp(uid, out SpriteComponent? pieceSprite))
                UpdateWoundableVisuals(part.Id, woundableVisuals, pieceSprite);
        }
    }

    private void OnWoundableIntegrityChanged(EntityUid uid, WoundableVisualsComponent component, ref WoundableIntegrityChangedEvent args)
    {
        var bodyPart = Comp<BodyPartComponent>(uid);
        if (!bodyPart.Body.HasValue)
        {
            if (TryComp(uid, out SpriteComponent? partSprite))
                UpdateWoundableVisuals(uid, component, partSprite);
            return;
        }

        if (TryComp(bodyPart.Body, out SpriteComponent? bodySprite))
            UpdateWoundableVisuals(uid, component, bodySprite);
    }

    private void AddDamageLayerToSprite(SpriteComponent spriteComponent, string sprite, string state, string mapKey, string? color = null)
    {
        var newLayer = spriteComponent.AddLayer(
            new SpriteSpecifier.Rsi(
                new ResPath(sprite),
                state
            ));
        spriteComponent.LayerMapSet(mapKey, newLayer);
        if (color != null)
            spriteComponent.LayerSetColor(newLayer, Color.FromHex(color));
        spriteComponent.LayerSetVisible(newLayer, false);
    }

    private void UpdateWoundableVisuals(EntityUid uid, WoundableVisualsComponent visuals, SpriteComponent sprite)
    {
        foreach (var group in visuals.DamageOverlayGroups!)
        {
            if (!sprite.LayerMapTryGet($"{visuals.OccupiedLayer}{group.Key}", out var damageLayer))
                continue;

            var severityPoint = _wound.GetWoundableSeverityPoint(uid, damageGroup: group.Key);
            UpdateDamageLayerState(sprite,
                damageLayer,
                $"{visuals.OccupiedLayer}_{group.Key}",
                severityPoint <= visuals.Thresholds.First() ? 0 : GetThreshold(severityPoint, visuals));
        }
        UpdateBleeding(uid, visuals, visuals.OccupiedLayer, sprite);
    }

    private void UpdateBleeding(EntityUid uid, WoundableVisualsComponent comp, Enum layer, SpriteComponent sprite)
    {
        if (!TryComp<BodyPartComponent>(uid, out var bodyPart))
            return;

        if (comp.BleedingOverlay == null)
        {
            if (!_body.TryGetParentBodyPart(uid, out var parentUid, out _))
                return;

            if (!_appearance.TryGetData<WoundVisualizerGroupData>(uid, WoundableVisualizerKeys.Wounds, out var wounds)
                || !_appearance.TryGetData<WoundVisualizerGroupData>(parentUid.Value, WoundableVisualizerKeys.Wounds, out var parentWounds))
                return;

            var woundList = new List<EntityUid>();
            woundList.AddRange(wounds.GroupList.Select(GetEntity));
            woundList.AddRange(parentWounds.GroupList.Select(GetEntity));

            var totalBleeds = (FixedPoint2) 0;
            foreach (var wound in woundList)
            {
                if (TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    totalBleeds += bleeds.BleedingAmount;
            }

            var symmetry = bodyPart.Symmetry == BodyPartSymmetry.Left ? "L" : "R";
            var partType = bodyPart.PartType == BodyPartType.Foot ? "Leg" : "Arm";

            var part = symmetry + partType;

            if (sprite.LayerMapTryGet($"{part}Bleeding", out var parentBleedingLayer))
                UpdateBleedingLayerState(
                sprite,
                parentBleedingLayer,
                part,
                totalBleeds,
                GetBleedingThreshold(totalBleeds, comp));
        }
        else
        {
            if (!_appearance.TryGetData<WoundVisualizerGroupData>(uid, WoundableVisualizerKeys.Wounds, out var wounds))
                return;

            var totalBleeds = (FixedPoint2) 0;
            foreach (var wound in wounds.GroupList.Select(GetEntity))
            {
                if (TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    totalBleeds += bleeds.BleedingAmount;
            }

            if (sprite.LayerMapTryGet($"{layer}Bleeding", out var bleedingLayer))
                UpdateBleedingLayerState(sprite,
                bleedingLayer,
                layer.ToString(),
                totalBleeds,
                GetBleedingThreshold(totalBleeds, comp));
        }
    }

    private FixedPoint2 GetThreshold(FixedPoint2 threshold, WoundableVisualsComponent comp)
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

    private BleedingSeverity GetBleedingThreshold(FixedPoint2 threshold, WoundableVisualsComponent comp)
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

    private void UpdateBleedingLayerState(SpriteComponent spriteComponent, int spriteLayer, string statePrefix, FixedPoint2 damage, BleedingSeverity threshold)
    {
        if (damage <= 0)
        {
            spriteComponent.LayerSetVisible(spriteLayer, false);
        }
        else
        {
            if (!spriteComponent[spriteLayer].Visible)
            {
                spriteComponent.LayerSetVisible(spriteLayer, true);
            }

            if (_random.Prob(AltBleedingSpriteChance))
            {
                var rsi = spriteComponent.LayerGetActualRSI(spriteLayer);

                if (rsi != null && rsi.TryGetState($"{statePrefix}_{threshold}_alt", out _))
                {
                    spriteComponent.LayerSetState(spriteLayer, $"{statePrefix}_{threshold}_alt");
                }
            }
            else
            {
                var rsi = spriteComponent.LayerGetActualRSI(spriteLayer);

                // ... for some reason?
                if (rsi != null && rsi.TryGetState($"{statePrefix}_{threshold}", out _))
                {
                    spriteComponent.LayerSetState(spriteLayer, $"{statePrefix}_{threshold}");
                }
            }
        }
    }

    private void UpdateDamageLayerState(SpriteComponent spriteComponent, int spriteLayer, string statePrefix, FixedPoint2 threshold)
    {
        if (threshold <= 0)
        {
            spriteComponent.LayerSetVisible(spriteLayer, false);
        }
        else
        {
            if (!spriteComponent[spriteLayer].Visible)
            {
                spriteComponent.LayerSetVisible(spriteLayer, true);
            }
            spriteComponent.LayerSetState(spriteLayer, $"{statePrefix}_{threshold}");
        }
    }
}
