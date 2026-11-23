using Content.Goobstation.Shared.Bongle;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Whitelist;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using System.Numerics;

namespace Content.Goobstation.Client.Bongle;

/// <summary>
/// Just check the summary for drone overlay system lol
/// Though this also tints the screen so hurray you get to look at nightvision code too.
/// </summary>
public sealed partial class SeethingOverlay : Overlay
{
    [Dependency] private IEntityManager _entity = default!;
    [Dependency] private IPlayerManager _player = default!;

    private readonly SharedTransformSystem _transform;
    private readonly EntityWhitelistSystem _whitelist;
    private readonly MobStateSystem _mobState;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public SeethingOverlay()
    {
        IoCManager.InjectDependencies(this);

        _transform = _entity.System<SharedTransformSystem>();
        _whitelist = _entity.System<EntityWhitelistSystem>();
        _mobState = _entity.System<MobStateSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var player = _player.LocalEntity;

        if (player is null || !_entity.TryGetComponent(player, out SeethingComponent? seething))
            return;

        var eye = args.Viewport.Eye;

        if (eye is null)
            return;

        var handle = args.WorldHandle;
        var mapId = args.MapId;
        var eyeRot = eye.Rotation;

        handle.DrawRect(args.WorldAABB, seething.OverlayColor);

        var query = _entity.EntityQueryEnumerator<MobStateComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out _, out var sprite, out var xform))
        {
            if (uid == player.Value || _mobState.IsDead(uid) || xform.MapID != mapId || !sprite.Visible)
                continue;

            if (_whitelist.IsWhitelistPassOrNull(seething.VisibleWhitelist, uid) && !_whitelist.IsWhitelistPass(seething.VisibleBlacklist, uid))
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
