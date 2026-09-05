using Content.Goobstation.Shared.Bloodsuckers.Components.Actions;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Goobstation.Client.Bloodsucker.Overlays;

public sealed class BloodsuckerOlfactionOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    private readonly SharedTransformSystem _transform;
    private readonly SpriteSystem _sprite;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    public BloodsuckerOlfactionOverlay()
    {
        IoCManager.InjectDependencies(this);
        _transform = _entity.System<SharedTransformSystem>();
        _sprite = _entity.System<SpriteSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var player = _player.LocalEntity;
        if (player == null)
            return;

        if (!_entity.TryGetComponent(player, out BloodsuckerOlfactionComponent? olf)
            || !olf.IsTracking || olf.TrackedTarget == null)
            return;

        if (!_entity.TryGetEntity(olf.TrackedTarget.Value, out var target))
            return;

        var handle = args.WorldHandle;
        var eye = args.Viewport.Eye;
        if (eye == null)
            return;

        var mapId = args.MapId;
        var eyeRot = eye.Rotation;

        // Red tint over entire screen
        handle.DrawRect(args.WorldAABB, new Color(120, 0, 0, 60));

        // Hide all living mobs that aren't the vampire or the target
        // by rendering them solid black, same as DroneVisionOverlay
        var query = _entity.EntityQueryEnumerator<SpriteComponent, TransformComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out var sprite, out var xform, out var mobState))
        {
            if (uid == player || uid == target)
                continue;

            if (mobState.CurrentState == MobState.Dead)
                continue;

            if (xform.MapID != mapId)
                continue;

            if (!sprite.Visible)
                continue;

            var pos = _transform.GetWorldPosition(xform);
            var rot = _transform.GetWorldRotation(xform);

            var original = sprite.Color;
            sprite.Color = Color.Black;
            sprite.Render(handle, eyeRot, rot, position: pos);
            sprite.Color = original;
        }

        handle.SetTransform(Matrix3x2.Identity);
    }
}
