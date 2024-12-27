using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server.Stack;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Physics;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics;
using Robust.Shared.Random;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Maps;
using Content.Shared.Tiles;
using Robust.Shared.Map.Components;
using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Server.Teleportation;

public sealed class TeleportSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PullingSystem _pullingSystem = default!;
    [Dependency] private readonly IAdminLogManager _alog = default!;
    [Dependency] private readonly StackSystem _stack = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IPrototypeManager _prot = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomTeleportOnUseComponent, UseInHandEvent>(OnUseInHand);

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    private void OnUseInHand(EntityUid uid, RandomTeleportOnUseComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<RandomTeleportComponent>(uid, out var teleport))
            return;

        _alog.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):actor} teleported with {ToPrettyString(uid)}");

        RandomTeleport(args.User, teleport);

        if (!component.ConsumeOnUse)
            return;

        if (TryComp<StackComponent>(uid, out var stack))
        {
            _stack.SetCount(uid, stack.Count - 1, stack);
            return;
        }

        // It's consumed on use and it's not a stack so delete it
        QueueDel(uid);
    }

    public void RandomTeleport(EntityUid uid, RandomTeleportComponent component)
    {
        RandomTeleport(uid, component.TeleportRadius, component.TeleportSound, component.TeleportAttempts);
    }

    public void RandomTeleport(EntityUid uid, float radius, SoundSpecifier sound, int attempts)
    {
        // We need stop the user from being pulled so they don't just get "attached" with whoever is pulling them.
        // This can for example happen when the user is cuffed and being pulled.
        if (TryComp<PullableComponent>(uid, out var pull) && _pullingSystem.IsPulled(uid, pull))
            _pullingSystem.TryStopPull(uid, pull);

        var xform = Transform(uid);
        var entityCoords = xform.Coordinates.ToMap(EntityManager, _xform);

        var targetCoords = new MapCoordinates();
        // Try to find a valid position to teleport to, teleport to whatever works if we can't
        // If attempts is 1 or less, degenerates to a completely random teleport
        for (var i = 0; i < Math.Max(attempts, 1); i++)
        {
            var distance = radius * MathF.Sqrt(_random.NextFloat()); // to get an uniform distribution
            targetCoords = entityCoords.Offset(_random.NextAngle().ToVec() * distance);

            // Prefer teleporting to grids
            if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var grid))
                continue;

            // If attempts is specified, whatever's being teleported probably does not want to be in your walls
            var valid = true;
            foreach (var entity in grid.GetAnchoredEntities(targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (valid)
                break;
        }

        _xform.SetWorldPosition(uid, targetCoords.Position);
        _audio.PlayPvs(sound, uid);
    }
}
