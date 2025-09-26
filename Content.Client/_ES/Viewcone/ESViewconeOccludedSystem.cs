using Content.Shared._ES.Viewcone;
using Content.Shared.MouseRotator;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Client._ES.Viewcone;

public sealed class ESViewconeOccludedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ESViewconeOccludedComponent, ComponentStartup>(OnOcclusionStart);
        SubscribeLocalEvent<ESViewconeOccludedComponent, ComponentShutdown>(OnOcclusionShutdown);
        SubscribeLocalEvent<ESViewconeOccludedComponent, AnchorStateChangedEvent>(OnOcclusionAnchorUpdate);
    }

    private void OnOcclusionStart(Entity<ESViewconeOccludedComponent> entity, ref ComponentStartup args)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(entity, out var sprite))
            return;
        entity.Comp.BaseAlpha = sprite.Color.A;
    }

    private void OnOcclusionShutdown(Entity<ESViewconeOccludedComponent> entity, ref ComponentShutdown args)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(entity, out var sprite))
            return;
        sprite.Color = sprite.Color.WithAlpha(entity.Comp.BaseAlpha);
    }

    private void OnOcclusionAnchorUpdate(Entity<ESViewconeOccludedComponent> entity, ref AnchorStateChangedEvent args)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(entity, out var sprite))
            return;

        if (!args.Anchored)
            return;
        sprite.Color = sprite.Color.WithAlpha(entity.Comp.BaseAlpha);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_timing.IsFirstTimePredicted)
            return;

        var playerEntity = _playerManager.LocalSession?.AttachedEntity;

        if (playerEntity == null)
            return;

        if (!_entityManager.TryGetComponent<ESViewconeComponent>(playerEntity, out var cone))
            return;

        var playerTransform = Transform(playerEntity.Value);
        var (playerPosition, playerRot) = _transform.GetWorldPositionRotation(playerTransform);

        if (_entityManager.TryGetComponent<MouseRotatorComponent>(playerEntity, out var mouse))
        {
            var mousePos = _eyeManager.PixelToMap(_inputManager.MouseScreenPosition);
            if (mousePos.MapId == playerTransform.MapID)
                playerRot = (mousePos.Position - _transform.GetMapCoordinates(playerTransform).Position).ToWorldAngle();
        }

        var radConeAngle = MathHelper.DegreesToRadians(cone.ConeAngle);
        var radConeFeather = MathHelper.DegreesToRadians(cone.ConeFeather);

        var query = AllEntityQuery<ESViewconeOccludedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (uid == playerEntity)
                continue;

            if (!_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite))
                continue;

            var xform = Transform(uid);
            var entPos = _transform.GetWorldPosition(xform);

            if (xform.MapID != playerTransform.MapID)
                continue;

            if (!_entityManager.HasComponent<MapComponent>(xform.ParentUid) && !_entityManager.HasComponent<MapGridComponent>(xform.ParentUid))
            {
                sprite.Color = sprite.Color.WithAlpha(comp.BaseAlpha);
                continue;
            }

            if (!comp.OccludeIfAnchored)
            {
                if (xform.Anchored)
                    continue;
            }

            var dist = entPos - playerPosition;
            var distLength = dist.Length();
            var angleDist = Angle.ShortestDistance(dist.ToWorldAngle(), playerRot);

            var angleAlpha = (float) Math.Clamp((Math.Abs(angleDist.Theta) - (radConeAngle * 0.5f)) + (radConeFeather * 0.5f), 0f, radConeFeather) / radConeFeather;
            var distAlpha = Math.Clamp((distLength - cone.ConeIgnoreRadius) + (cone.ConeIgnoreFeather * 0.5f), 0f, cone.ConeIgnoreFeather) / cone.ConeIgnoreFeather;
            var targetAlpha = Math.Max(1f - angleAlpha, 1f - distAlpha);

            sprite.Color = sprite.Color.WithAlpha(targetAlpha * comp.BaseAlpha);
        }
    }
}
