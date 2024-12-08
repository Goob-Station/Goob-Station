using Content.Server._Lavaland.Mobs.Bosses.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;

namespace Content.Server._Lavaland.Mobs.Bosses;

/// <summary>
///     Chaser works as a self replicator.
///     It searches for the player, picks a neat position and spawns itself with something else
///     (in our case hierophant damaging square).
/// </summary>
public sealed partial class HierophantChaserSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var eqe = EntityQueryEnumerator<HierophantChaserComponent>();
        var chasers = new List<(EntityUid, HierophantChaserComponent)>();
        while (eqe.MoveNext(out var uid, out var comp))
        {
            var delta = frameTime * comp.Speed;
            comp.CooldownTimer -= delta;

            if (comp.CooldownTimer <= 0)
            {
                // if i do it here i will get an exception so i'll just add them to a list.
                chasers.Add((uid, comp));
            }
        }

        if (chasers.Count == 0)
            return;

        foreach (var chaser in chasers)
        {
            chaser.Item2.CooldownTimer = chaser.Item2.BaseCooldown;
            Cycle((chaser.Item1, chaser.Item2));
        }
    }

    /// <summary>
    ///     Crawl one tile away from it's initial position.
    ///     Replicate itself and the prototype designated.
    ///     Delete itself afterwards.
    /// </summary>
    private void Cycle(Entity<HierophantChaserComponent> ent)
    {
        var xform = Transform(ent);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) xform.GridUid!, grid);

        // get it's own map position
        var pos = _xform.GetWorldPosition(ent);
        var confines = new Box2(pos - Vector2.One, pos + Vector2.One);

        // get random position tiles pool
        var randomPos = _map.GetLocalTilesIntersecting(ent, grid, confines).ToList();
        var deltaPos = Vector2.Zero;

        // if there is a target get it's position delta instead
        if (ent.Comp.Target != null)
        {
            deltaPos = _xform.GetWorldPosition((EntityUid) ent.Comp.Target) - pos;
        }

        // if the target is still missing we'll just pick a random tile
        if (deltaPos == Vector2.Zero && randomPos.Count > 0)
            deltaPos = _random.Pick(randomPos).GridIndices;

        // translate it
        deltaPos = TranslateDelta(deltaPos);

        // spawn damaging square and set new position
        Spawn(ent.Comp.SpawnPrototype, xform.Coordinates);
        _xform.SetWorldPosition(ent, pos + deltaPos);

        // handle steps
        ent.Comp.Steps += 1;
        if (ent.Comp.Steps >= ent.Comp.MaxSteps)
            QueueDel(ent);
    }

    private Vector2i TranslateDelta(Vector2 delta)
    {
        int x = 0, y = 0;

        x = (int) Math.Clamp(MathF.Round(delta.X, 0), -1, 1);
        y = (int) Math.Clamp(MathF.Round(delta.Y, 0), -1, 1);

        // made for square-like movement
        if (Math.Abs(x) >= Math.Abs(y)) y = 0;
        else x = 0;

        var translated = new Vector2i(x, y);

        return translated;
    }
}
