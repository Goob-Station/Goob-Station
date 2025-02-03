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
    ///     Crawl one tile away from it's initial position.
    ///     Replicate itself and the prototype designated.
    ///     Delete itself afterwards.
    /// </summary>
    private void Cycle(Entity<HierophantChaserComponent, TransformComponent?> ent)
    {
        if (!Resolve<TransformComponent>(ent, ref ent.Comp2, false))
            return;

        var xform = Transform(ent);
        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
        {
            return;
        }

        var gridEnt = (xform.GridUid.Value, grid);

        // get tile position of the chaser
        if (!_xform.TryGetGridTilePosition((ent.Owner, ent.Comp2), out var tilePos, grid))
        {
            QueueDel(ent);
            return;
        }

        var deltaPos = Vector2i.Zero;

        // if there is a target get it's position delta instead
        if (ent.Comp1.Target != null &&
            !TerminatingOrDeleted(ent.Comp1.Target))
        {
            var target = ent.Comp1.Target.Value;

            // get tile position of the target
            if (!_xform.TryGetGridTilePosition((target, Transform(target)), out var tileTargetPos, grid))
            {
                // If not on our grid that is equal to no target
                QueueDel(ent);
                return;
            }

            deltaPos = tileTargetPos;
        }

        var directions = new List<Vector2i>
        {
            new(1, 0),
            new(0, 1),
            new(-1, 0),
            new(0, -0),
        };

        // if the target is still missing we move randomly
        if (deltaPos == Vector2.Zero)
        {
            deltaPos = _random.Pick(directions);
        }

        // translate it
        deltaPos = TranslateDelta(deltaPos);

        // spawn damaging square and set new position
        var newPos = _map.GridTileToWorld(xform.GridUid.Value, grid, tilePos + deltaPos);
        Spawn(ent.Comp1.SpawnPrototype, newPos);
        _xform.SetMapCoordinates(ent, newPos);

        // handle steps
        ent.Comp1.Steps += 1;
        if (ent.Comp1.Steps >= ent.Comp1.MaxSteps)
            QueueDel(ent);
    }

    private Vector2i TranslateDelta(Vector2 delta)
    {
        int x, y;

        x = (int) Math.Clamp(MathF.Round(delta.X, 0), -1, 1);
        y = (int) Math.Clamp(MathF.Round(delta.Y, 0), -1, 1);

        // made for square-like movement
        if (Math.Abs(x) >= Math.Abs(y))
            y = 0;
        else
            x = 0;

        var translated = new Vector2i(x, y);

        return translated;
    }
}
