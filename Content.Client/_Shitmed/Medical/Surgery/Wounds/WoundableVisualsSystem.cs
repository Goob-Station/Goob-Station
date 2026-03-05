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

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Client._Shitmed.Medical.Surgery.Wounds;

/// <summary>
/// Handles visual representation of wounds and damage on body parts
/// </summary>
public sealed class WoundableVisualsSystem : VisualizerSystem<WoundableVisualsComponent>
{
    #region Dependencies
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly WoundSystem _wound = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    #endregion
    #region Constants
    private const float AltBleedingSpriteChance = 0.15f;
    private const string BleedingSuffix = "Bleeding";
    private const string MinorSuffix = "Minor";
    #endregion
    #region Initialization
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WoundableVisualsComponent, ComponentInit>(InitializeEntity, after: [typeof(WoundSystem)]);
        SubscribeLocalEvent<WoundableVisualsComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartRemovedEvent>(OnWoundableRemoved);
        SubscribeLocalEvent<WoundableVisualsComponent, BodyPartAddedEvent>(OnWoundableConnected);
        SubscribeLocalEvent<WoundableVisualsComponent, WoundableIntegrityChangedEvent>(OnWoundableIntegrityChanged);
    }

    private void InitializeEntity(Entity<WoundableVisualsComponent> ent, ref ComponentInit args)
    {
        InitDamage(ent);
        InitBleeding(ent);
    }

    private void InitBleeding(Entity<WoundableVisualsComponent> ent)
    {
        if (ent.Comp.BleedingOverlay == null)
            return;
        AddDamageLayerToSprite(ent.Owner, ent.Comp.BleedingOverlay, BuildStateKey(ent.Comp.OccupiedLayer, MinorSuffix), BuildLayerKey(ent.Comp.OccupiedLayer, BleedingSuffix));
    }

    private void InitDamage(Entity<WoundableVisualsComponent> ent)
    {
        if (ent.Comp.DamageOverlayGroups is null)
            return;
        foreach (var (group, sprite) in ent.Comp.DamageOverlayGroups)
            AddDamageLayerToSprite(ent.Owner,
                sprite.Sprite,
                BuildStateKey(ent.Comp.OccupiedLayer, group, "100"),
                BuildLayerKey(ent.Comp.OccupiedLayer, group),
                sprite.Color);
    }
    #endregion
    #region Event Handlers

    private void OnAfterAutoHandleState(Entity<WoundableVisualsComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (!TryComp(ent, out SpriteComponent? partSprite))
            return;

        UpdateWoundableVisuals(ent, (ent, partSprite));
    }

    private void OnWoundableConnected(Entity<WoundableVisualsComponent> ent, ref BodyPartAddedEvent args)
    {
        var bodyPart = args.Part.Comp;
        if (bodyPart.Body is not { } bodyUid || !HasComp<HumanoidAppearanceComponent>(bodyUid))
            return;

        if (ent.Comp.DamageOverlayGroups != null)
        {
            foreach (var (group, sprite) in ent.Comp.DamageOverlayGroups)
                if (!_sprite.LayerMapTryGet(bodyUid, BuildLayerKey(ent.Comp.OccupiedLayer, group), out _, false))
                {
                    AddDamageLayerToSprite(bodyUid,
                        sprite.Sprite,
                        BuildStateKey(ent.Comp.OccupiedLayer, group, "100"),
                        BuildLayerKey(ent.Comp.OccupiedLayer, group),
                        sprite.Color);
                }
        }
        if (!_sprite.LayerMapTryGet(bodyUid, BuildLayerKey(ent.Comp.OccupiedLayer, BleedingSuffix), out _, false)
            && ent.Comp.BleedingOverlay != null)
        {
            AddDamageLayerToSprite(bodyUid,
                ent.Comp.BleedingOverlay,
                BuildStateKey(ent.Comp.OccupiedLayer, MinorSuffix),
                BuildLayerKey(ent.Comp.OccupiedLayer, BleedingSuffix));
        }

        UpdateWoundableVisuals(ent, bodyUid);
    }

    private void OnWoundableRemoved(Entity<WoundableVisualsComponent> ent, ref BodyPartRemovedEvent args)
    {
        var body = args.Part.Comp.Body;
        if (body is null)
            return;

        foreach (var part in _body.GetBodyPartChildren(ent))
        {
            if (!TryComp<WoundableVisualsComponent>(part.Id, out var woundableVisuals))
                continue;
            RemoveWoundableLayers(body.Value, woundableVisuals);
            if (TryComp(ent, out SpriteComponent? pieceSprite))
                UpdateWoundableVisuals((part.Id, woundableVisuals), (ent, pieceSprite));
        }
    }

    private void OnWoundableIntegrityChanged(Entity<WoundableVisualsComponent> ent, ref WoundableIntegrityChangedEvent args)
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
    #endregion
    #region Layer Management
    private void RemoveWoundableLayers(Entity<SpriteComponent?> ent, WoundableVisualsComponent visuals)
    {
        if (visuals.DamageOverlayGroups == null || !Resolve(ent,ref ent.Comp))
            return;

        foreach (var (group, _) in visuals.DamageOverlayGroups)
        {
            var layerKey = BuildLayerKey(visuals.OccupiedLayer, group);
            if (!_sprite.LayerMapTryGet(ent, layerKey, out var layer, false))
                continue;
            _sprite.LayerSetVisible(ent, layer, false);
            _sprite.RemoveLayer(ent, layer);
            _sprite.LayerMapRemove(ent, layerKey);
        }

        var bleedingKey = BuildLayerKey(visuals.OccupiedLayer, BleedingSuffix);
        if (!_sprite.LayerMapTryGet(ent, bleedingKey, out var bleedLayer, false))
            return;
        _sprite.LayerSetVisible(ent, bleedLayer, false);
        _sprite.RemoveLayer(ent, bleedLayer, out _, false);
        _sprite.LayerMapRemove(ent, bleedingKey, out _);
    }

    private void AddDamageLayerToSprite(Entity<SpriteComponent?> ent,
        string sprite,
        string state,
        string mapKey,
        string? color = null)
    {
        if (!Resolve(ent, ref ent.Comp) ||_sprite.LayerMapTryGet(ent, mapKey, out _, false)) // prevent dupes
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
    #endregion
    #region Visual Updates
    private void UpdateWoundableVisuals(Entity<WoundableVisualsComponent> visuals, Entity<SpriteComponent?> sprite)
    {
        UpdateDamageVisuals(visuals, sprite);
        UpdateBleedingVisuals(visuals, sprite);
    }

    private void UpdateDamageVisuals(Entity<WoundableVisualsComponent> visuals, Entity<SpriteComponent?> sprite)
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
    private void UpdateBleedingVisuals(Entity<WoundableVisualsComponent> ent, Entity<SpriteComponent?> sprite)
    {
        if (!TryComp<BodyPartComponent>(ent, out var bodyPart))
            return;

        if (ent.Comp.BleedingOverlay is null)
        {
            UpdateParentBleedingVisuals(ent, bodyPart, sprite);
            return;
        }

        UpdateOwnBleedingVisuals(ent, sprite);
    }

    private void UpdateParentBleedingVisuals(
        Entity<WoundableVisualsComponent> woundable,
        BodyPartComponent bodyPart,
        Entity<SpriteComponent?> sprite)
    {
        if (!_body.TryGetParentBodyPart(woundable, out var parentUid, out _))
            return;

        var partKey = GetLimbBleedingKey(bodyPart);
        var layerKey = BuildLayerKey(partKey, BleedingSuffix);
        var hasWounds = TryGetWoundData(woundable.Owner, out var wounds);
        var hasParentWounds = TryGetWoundData(parentUid.Value, out var parentWounds);

        if (!hasWounds && !hasParentWounds)
        {
            if (_sprite.LayerMapTryGet(sprite, layerKey, out var layer, false))
                _sprite.LayerSetVisible(sprite, layer, false);
            return;
        }

        var totalBleeds = FixedPoint2.Zero;
        if (hasWounds)
            totalBleeds += CalculateTotalBleeding(wounds);
        if (hasParentWounds)
            totalBleeds += CalculateTotalBleeding(parentWounds);

        if (!_sprite.LayerMapTryGet(sprite, layerKey, out var bleedingLayer, false))
            return;

        var threshold = CalculateBleedingThreshold(totalBleeds, woundable.Comp);
        UpdateBleedingLayerState(sprite, bleedingLayer, partKey, totalBleeds, threshold);
    }

    private void UpdateOwnBleedingVisuals(Entity<WoundableVisualsComponent> woundable, Entity<SpriteComponent?> sprite)
    {
        var layerKey = BuildLayerKey(woundable.Comp.OccupiedLayer, BleedingSuffix);

        if (!TryGetWoundData(woundable.Owner, out var wounds))
        {
            if (_sprite.LayerMapTryGet(sprite, layerKey, out var layer, false))
                _sprite.LayerSetVisible(sprite, layer, false);
            return;
        }

        var totalBleeds = CalculateTotalBleeding(wounds);
        if (!_sprite.LayerMapTryGet(sprite, layerKey, out var bleedingLayer, false))
            return;
        var threshold = CalculateBleedingThreshold(totalBleeds, woundable.Comp);
        UpdateBleedingLayerState(sprite, bleedingLayer, woundable.Comp.OccupiedLayer.ToString(), totalBleeds, threshold);
    }

    #endregion
    #region Helper Methods
    private void SetLayerVisible(Entity<SpriteComponent?> sprite, int layer, bool visibility)
    {
        if (_sprite.TryGetLayer(sprite, layer, out var layerData, false) && layerData.Visible != visibility)
            _sprite.LayerSetVisible(sprite, layer, visibility);
    }

    private bool TryGetWoundData(Entity<WoundableVisualsComponent?> entity, [NotNullWhen(true)] out WoundVisualizerGroupData? wounds)
    {
        wounds = null;
        if (!Resolve(entity, ref entity.Comp) || !_appearance.TryGetData(entity.Owner, WoundableVisualizerKeys.Wounds, out wounds))
            return false;
        if (wounds.GroupList.Count != 0)
            return true;
        wounds = null;
        return false;
    }

    private FixedPoint2 CalculateTotalBleeding(params WoundVisualizerGroupData?[] woundGroups)
    {
        var total = FixedPoint2.Zero;

        foreach (var group in woundGroups)
        {
            if (group == null || group.GroupList.Count == 0)
                continue;

            foreach (var wound in group.GroupList.Select(GetEntity))
            {
                if (TryComp<BleedInflicterComponent>(wound, out var bleeds))
                    total += bleeds.BleedingAmount;
            }
        }

        return total;
    }
    private static BleedingSeverity CalculateBleedingThreshold(FixedPoint2 bleeding, WoundableVisualsComponent comp)
    {
        var nearestSeverity = BleedingSeverity.Minor;

        foreach (var (severity, value) in comp.BleedingThresholds.OrderByDescending(kv => kv.Value))
        {
            if (bleeding < value)
                continue;
            nearestSeverity = severity;
            break;
        }

        return nearestSeverity;
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

    private void UpdateBleedingLayerState(Entity<SpriteComponent?> sprite,
        int spriteLayer,
        string statePrefix,
        FixedPoint2 damage,
        BleedingSeverity threshold)
    {
        if (!Resolve(sprite, ref sprite.Comp))
            return;

        if (damage <= 0)
        {
            SetLayerVisible(sprite, spriteLayer, false);
            return;
        }

        SetLayerVisible(sprite, spriteLayer, true);

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

    private static string GetLimbBleedingKey(BodyPartComponent bodyPart)
    {
        var symmetry = bodyPart.Symmetry == BodyPartSymmetry.Left ? "L" : "R";
        var partType = bodyPart.PartType == BodyPartType.Foot ? "Leg" : "Arm";
        return $"{symmetry}{partType}";
    }


    private static string BuildLayerKey(Enum baseLayer, string suffix) => $"{baseLayer}{suffix}";
    private static string BuildLayerKey(string baseLayer, string suffix) => $"{baseLayer}{suffix}";
    private static string BuildStateKey(Enum baseLayer, string suffix) => $"{baseLayer}_{suffix}";
    private static string BuildStateKey(Enum baseLayer, string group, string suffix) => $"{baseLayer}_{group}_{suffix}";

    #endregion
}
