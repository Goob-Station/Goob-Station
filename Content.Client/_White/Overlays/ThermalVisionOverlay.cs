using System.Linq;
using System.Numerics;
using Content.Client.Stealth;
using Content.Shared._White.Overlays;
using Content.Shared.Body.Components;
using Content.Shared.Stealth.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;

namespace Content.Client._White.Overlays;

public sealed class ThermalVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private readonly TransformSystem _transform;
    private readonly OccluderSystem _occluder;
    private readonly StealthSystem _stealth;
    private readonly ContainerSystem _container;

    public override bool RequestScreenTexture => true;
    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<ThermalVisionRenderEntry> _entries = [];

    public ThermalVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _occluder = _entity.System<OccluderSystem>();
        _stealth = _entity.System<StealthSystem>();

        ZIndex = -1;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture is null
            || _players.LocalEntity == null
            || !_entity.TryGetComponent<ThermalVisionComponent>(_players.LocalEntity.Value, out var component)
            || !component.IsActive)
            return;

        var worldHandle = args.WorldHandle;
        var eye = args.Viewport.Eye;

        if (eye == null)
            return;

        var mapId = eye.Position.MapId;
        var eyeRot = eye.Rotation;

        _entries.Clear();
        var entities = _entity.EntityQueryEnumerator<BodyComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out var body, out var sprite, out var xform))
        {
            if (!CanSee(uid, sprite, body))
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

            if (_entries.Any(e => e.Ent.Item1 == entity))
                continue;

            _entries.Add(new ThermalVisionRenderEntry((entity, sprite, xform, body), mapId, eyeRot));
        }

        foreach (var entry in _entries)
        {
            Render(entry.Ent, entry.Map, worldHandle, entry.EyeRot);
        }

        worldHandle.SetTransform(Matrix3.Identity);
    }

    private void Render(Entity<SpriteComponent, TransformComponent, BodyComponent> ent,
        MapId? map,
        DrawingHandleWorld handle,
        Angle eyeRot)
    {
        var (uid, sprite, xform, body) = ent;
        if (xform.MapID != map || HasOccluders(uid) || !CanSee(uid, sprite, body))
            return;

        var position = _transform.GetWorldPosition(xform);
        var rotation = _transform.GetWorldRotation(xform);

        sprite.Render(handle, eyeRot, rotation, position: position);
    }

    private bool CanSee(EntityUid uid, SpriteComponent sprite, BodyComponent body)
    {
        return sprite.Visible
               && body.ThermalVisibility
               && (!_entity.TryGetComponent(uid, out StealthComponent? stealth)
                   || _stealth.GetVisibility(uid, stealth) > 0.5f);
    }

    private bool HasOccluders(EntityUid uid)
    {
        var mapCoordinates = _transform.GetMapCoordinates(uid);
        var occluders = _occluder.QueryAabb(mapCoordinates.MapId,
            Box2.CenteredAround(mapCoordinates.Position, new Vector2(0.3f, 0.3f)));

        return occluders.Any(o => o.Component.Enabled);
    }
}

public record struct ThermalVisionRenderEntry(
    (EntityUid, SpriteComponent, TransformComponent, BodyComponent) Ent,
    MapId? Map,
    Angle EyeRot);
