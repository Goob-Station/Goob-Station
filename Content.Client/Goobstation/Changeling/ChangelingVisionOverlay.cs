using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Content.Shared.Changeling;

namespace Content.Client.Changeling;

public sealed class ChangelingVisionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    private readonly ContainerSystem _container;
    private readonly TransformSystem _transform;
    private readonly EntityQuery<AbsorbableComponent> _absorbableQuery;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    private readonly List<NightVisionRenderEntry> _entries = new();

    public ChangelingVisionOverlay()
    {
        IoCManager.InjectDependencies(this);

        _container = _entity.System<ContainerSystem>();
        _transform = _entity.System<TransformSystem>();
        _absorbableQuery = _entity.GetEntityQuery<AbsorbableComponent>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (!_entity.TryGetComponent(_players.LocalEntity, out AbsorbableComponent? nightVision))
        {
            return;
        }

        var handle = args.WorldHandle;
        var eye = args.Viewport.Eye;
        var eyeRot = eye?.Rotation ?? default;

        _entries.Clear();
        var entities = _entity.EntityQueryEnumerator<AbsorbableComponent, SpriteComponent, TransformComponent>();
        while (entities.MoveNext(out var uid, out var visible, out var sprite, out var xform))
        {
            _entries.Add(new NightVisionRenderEntry((uid, sprite, xform),
                eye?.Position.MapId,
                eyeRot,
                false,
                (int)3,
                null
                ));
        }

        _entries.Sort(SortPriority);

        foreach (var entry in _entries)
        {
            Render(entry.Ent,
                entry.Map,
                handle,
                entry.EyeRot
                );
        }

        handle.SetTransform(Matrix3x2.Identity);
    }

    private static int SortPriority(NightVisionRenderEntry x, NightVisionRenderEntry y)
    {
        return x.Priority.CompareTo(y.Priority);
    }

    private void Render(Entity<SpriteComponent, TransformComponent> ent,
        MapId? map,
        DrawingHandleWorld handle,
        Angle eyeRot,
        bool seeThroughContainers = true,
        float? transparency = null)
    {
        var (uid, sprite, xform) = ent;
        if (xform.MapID != map)
            return;

        var seeThrough = seeThroughContainers && !_absorbableQuery.HasComp(uid);
        if (!seeThrough && _container.IsEntityOrParentInContainer(uid, xform: xform))
            return;

        var (position, rotation) = _transform.GetWorldPositionRotation(xform);

        var colorCache = sprite.Color;
        if (transparency != null)
        {
            var color = sprite.Color * Color.White.WithAlpha(transparency.Value);
            sprite.Color = color;
        }
        sprite.Render(handle, eyeRot, rotation, position: position);
        if (transparency != null)
        {
            sprite.Color = colorCache;
        }
    }
}

public record struct NightVisionRenderEntry(
    (EntityUid, SpriteComponent, TransformComponent) Ent,
    MapId? Map,
    Angle EyeRot,
    bool NightVisionSeeThroughContainers,
    int Priority,
    float? Transparency);
