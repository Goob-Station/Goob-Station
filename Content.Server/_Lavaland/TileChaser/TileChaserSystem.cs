// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using System.Numerics;
using Content.Shared._Lavaland.Anger.Systems;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.TileChaser;

namespace Content.Server._Lavaland.TileChaser;

/// <summary>
///     Chaser works as a self replicator.
///     It searches for the player, picks a neat position and spawns itself with something else
/// </summary>
public sealed class TileChaserSystem : EntitySystem
{
    [Dependency] private readonly AngerSystem _anger = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private static readonly Vector2i[] Directions =
    {
        new( 1,  0),
        new( 0,  1),
        new(-1,  0),
        new ( 0, -1),
    };

    private EntityQuery<TileChaserComponent> _chaserQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AngerTileChaserComponent, SpawnedByActionEvent>(OnSpawned);
        _chaserQuery = GetEntityQuery<TileChaserComponent>();
    }

    private void OnSpawned(Entity<AngerTileChaserComponent> ent, ref SpawnedByActionEvent args)
    {
        if (!_chaserQuery.TryComp(ent.Owner, out var chaserComp))
            return;

        var anger = ent.Comp;
        chaserComp.MaxSteps = _anger.GetAngerScale(args.User, anger.StepsRange.X, anger.StepsRange.Y, anger.Inverse);
        chaserComp.Speed = _anger.GetAngerScale(args.User, anger.SpeedRange.X, anger.SpeedRange.Y, anger.Inverse);
        chaserComp.Target = args.Target;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<TileChaserComponent, TransformComponent>();
        while (eqe.MoveNext(out var uid, out var comp, out var xform))
        {
            var delta = frameTime * comp.Speed;
            comp.CooldownTimer -= delta;

            if (comp.CooldownTimer <= 0)
            {
                Cycle((uid, comp, xform));
                comp.CooldownTimer = comp.BaseCooldown;
            }
        }
    }

    /// <summary>
    ///     Crawl one tile away from its initial position.
    ///     Replicate itself and the prototype designated.
    ///     Delete itself afterwards.
    /// </summary>
    private void Cycle(Entity<TileChaserComponent, TransformComponent> ent)
    {
        var xform = ent.Comp2;
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        // Get the chaser’s current tile position.
        if (!_xform.TryGetGridTilePosition((ent.Owner, ent.Comp2), out var tilePos, grid))
        {
            QueueDel(ent);
            return;
        }

        var deltaPos = _random.Pick(Directions);

        // If there is a valid target, calculate the delta toward the target.
        if (ent.Comp1.Target != null && !TerminatingOrDeleted(ent.Comp1.Target))
        {
            var target = ent.Comp1.Target.Value;

            // Attempt to get the target’s tile position.
            if (!_xform.TryGetGridTilePosition((target, Transform(target)), out var tileTargetPos, grid))
            {
                // If target is not on the same grid, schedule deletion.
                QueueDel(ent);
                return;
            }

            // This monstrosity is to make snake-like movement
            if (tileTargetPos.Y != tilePos.Y)
                tileTargetPos.X = tilePos.X;
            else if (tileTargetPos.Y != tilePos.Y)
                tileTargetPos.X = tilePos.X;
            else
                tileTargetPos += _random.Pick(Directions);

            // Don't forget kids, a DELTA is a difference between two things.
            deltaPos = tileTargetPos - tilePos;
        }

        // Translate the delta to ensure single-tile, axis-aligned movement.
        deltaPos = TranslateDelta(deltaPos);

        // Calculate the new world position based on grid coordinates.
        var newPos = _map.GridTileToWorld(xform.GridUid.Value, grid, tilePos + deltaPos);

        Spawn(ent.Comp1.Spawn, newPos);
        _xform.SetMapCoordinates(ent, newPos);

        // Increment steps and delete the entity if the maximum is reached.
        ent.Comp1.Steps += 1;
        if (ent.Comp1.Steps >= ent.Comp1.MaxSteps)
            QueueDel(ent);
    }

    /// <summary>
    /// Clamps and adjusts the delta to enforce square-like (axis-aligned) movement.
    /// </summary>
    private Vector2i TranslateDelta(Vector2 delta)
    {
        delta = Vector2.Clamp(Vector2.Round(delta), new Vector2(-1, -1), new Vector2(1, 1));

        return Math.Abs(delta.X) >= Math.Abs(delta.Y)
            ? new Vector2i((int)delta.X, 0)
            : new Vector2i(0, (int)delta.Y);
    }
}
