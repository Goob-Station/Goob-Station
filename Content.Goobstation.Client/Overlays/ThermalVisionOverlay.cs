// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Body.Components;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Goobstation.Client.Overlays;

public sealed class ThermalVisionOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly TransformSystem _transform;
    private readonly SpriteSystem _sprite;
    private readonly ContainerSystem _container;
    private readonly SharedPointLightSystem _light;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<ThermalVisionRenderEntry> _entries = [];

    private EntityUid? _lightEntity;

    public float LightRadius;

    public ThermalVisionComponent? Comp;

    public ThermalVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _sprite = _entity.System<SpriteSystem>();
        _light = _entity.System<SharedPointLightSystem>();

        ZIndex = -1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null || Comp is null)
            return;

        var worldHandle = args.WorldHandle;
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var player = _player.LocalEntity;

        if (!_entity.TryGetComponent(player, out TransformComponent? playerXform))
            return;

        var accumulator = Math.Clamp(Comp.PulseAccumulator, 0f, Comp.PulseTime);
        var alpha = Comp.PulseTime <= 0f ? 1f : float.Lerp(1f, 0f, accumulator / Comp.PulseTime);

        // Thermal vision grants some night vision (clientside light)
        if (LightRadius > 0)
        {
            _lightEntity ??= _entity.SpawnAttachedTo(null, playerXform.Coordinates);
            _transform.SetParent(_lightEntity.Value, player.Value);
            var light = _entity.EnsureComponent<PointLightComponent>(_lightEntity.Value);
            _light.SetRadius(_lightEntity.Value, LightRadius, light);
            _light.SetEnergy(_lightEntity.Value, alpha, light);
            _light.SetColor(_lightEntity.Value, Comp.Color, light);
        }
        else
            ResetLight();

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;

        _entries.Clear();
        var entities = _entity.EntityQueryEnumerator<BodyComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out var body, out var sprite, out var xform))
        {
            if (!CanSee(uid, sprite) || !body.ThermalVisibility)
                continue;

            var entity = uid;

            if (_container.TryGetOuterContainer(uid, xform, out var container))
            {
                var owner = container.Owner;
                if (_entity.TryGetComponent<SpriteComponent>(owner, out var ownerSprite)
                    && _entity.TryGetComponent<TransformComponent>(owner, out var ownerXform))
                {
                    entity = owner;
                    sprite = ownerSprite;
                    xform = ownerXform;
                }
            }

            if (_entries.Any(e => e.Ent.Owner == entity))
                continue;

            _entries.Add(new ThermalVisionRenderEntry((entity, sprite, xform), mapId, eyeRot));
        }

        foreach (var entry in _entries)
        {
            Render(entry.Ent, entry.Map, worldHandle, entry.EyeRot, Comp.Color, Comp.ThermalShader, alpha);
        }

        worldHandle.SetTransform(Matrix3x2.Identity);
    }

    private void Render(Entity<SpriteComponent, TransformComponent> ent,
        MapId? map,
        DrawingHandleWorld handle,
        Angle eyeRot,
        Color color,
        string? shader,
        float alpha)
    {
        var (uid, sprite, xform) = ent;
        if (xform.MapID != map || !CanSee(uid, sprite))
            return;

        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        var originalColor = sprite.Color;
        Dictionary<int, (ShaderInstance? shader, Color color)> layerData = new();
        if (shader != null)
        {
            // Layer shaders break handle shader so we have to do this. It has a side effect of clothing not rendering
            // on some species or on female characters but its fine cause shader itself makes things hard to see
            var allLayers = sprite.AllLayers.ToList();
            for (var i = 0; i < allLayers.Count; i++)
            {
                if (allLayers[i] is not SpriteComponent.Layer { Visible: true } layer)
                    continue;

                if (layer.ShaderPrototype?.Id is "DisplacedDraw" or "DisplacedStencilDraw")
                    _sprite.LayerSetVisible((uid, sprite), i, false);

                layerData[i] = (layer.Shader, layer.Color);
                layer.Shader = null;
                _sprite.LayerSetColor(layer, Color.White.WithAlpha(layer.Color.A));
            }

            _sprite.SetColor((uid, sprite), Color.White.WithAlpha(alpha));
            handle.UseShader(_protoMan.Index<ShaderPrototype>(shader).Instance());
        }
        else
            _sprite.SetColor((uid, sprite), color.WithAlpha(alpha));
        _sprite.RenderSprite((uid, sprite), handle, eyeRot, rotation, position);
        _sprite.SetColor((uid, sprite), originalColor);
        handle.UseShader(null);
        foreach (var (key, value) in layerData)
        {
            ((SpriteComponent.Layer) sprite[key]).Shader = value.shader;
            _sprite.LayerSetColor((uid, sprite), key, value.color);
            _sprite.LayerSetVisible((uid, sprite), key, true);
        }
    }

    private bool CanSee(EntityUid uid, SpriteComponent sprite)
    {
        return sprite.Visible && (!_entity.TryGetComponent(uid, out StealthComponent? stealth) ||
                                  !stealth.ThermalsImmune); // Goobstation - thermals ability to see invisible entities
    }

    public void ResetLight(bool checkFirstTimePredicted = true)
    {
        if (_lightEntity == null || checkFirstTimePredicted && !_timing.IsFirstTimePredicted)
            return;

        _entity.DeleteEntity(_lightEntity);
        _lightEntity = null;
    }
}

public record struct ThermalVisionRenderEntry(
    Entity<SpriteComponent, TransformComponent> Ent,
    MapId? Map,
    Angle EyeRot);
