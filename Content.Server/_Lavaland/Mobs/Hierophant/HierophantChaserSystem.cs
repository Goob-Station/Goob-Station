using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using System.Numerics;
using Content.Server._Lavaland.Mobs.Hierophant.Components;

namespace Content.Server._Lavaland.Mobs.Hierophant;

/// <summary>
///     Chaser works as a self replicator.
///     It searches for the player, picks a neat position and spawns itself with something else
///     (in our case hierophant damaging square).
/// </summary>
public sealed class HierophantChaserSystem : EntitySystem
{
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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantChaserComponent>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (TerminatingOrDeleted(uid))
                continue;

            var delta = frameTime * comp.Speed;
            comp.CooldownTimer -= delta;

            if (comp.CooldownTimer <= 0)
            {
                Cycle((uid, comp));
                comp.CooldownTimer = comp.BaseCooldown;
            }
        }
    }

    /// <summary>
    ///     Crawl one tile away from its initial position.
    ///     Replicate itself and the prototype designated.
    ///     Delete itself afterwards.
    /// </summary>
    private void Cycle(Entity<HierophantChaserComponent, TransformComponent?> ent)
    {
        if (!Resolve<TransformComponent>(ent, ref ent.Comp2, false))
            return;

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
            {
                tileTargetPos.X = tilePos.X;
            }
            else if (tileTargetPos.Y != tilePos.Y)
            {
                tileTargetPos.X = tilePos.X;
            }
            else
            {
                tileTargetPos += _random.Pick(Directions);
            }

            // Don't forget kids, a DELTA is a difference between two things.
            deltaPos = tileTargetPos - tilePos;
        }

        // Translate the delta to ensure single-tile, axis-aligned movement.
        deltaPos = TranslateDelta(deltaPos);

        // Calculate the new world position based on grid coordinates.
        var newPos = _map.GridTileToWorld(xform.GridUid.Value, grid, tilePos + deltaPos);

        Spawn(ent.Comp1.SpawnPrototype, newPos);
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
