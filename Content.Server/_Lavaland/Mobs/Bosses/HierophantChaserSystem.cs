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
        while (eqe.MoveNext(out var uid, out var comp))
        {
            if (comp.Steps >= comp.MaxSteps)
                QueueDel(uid);

            var delta = frameTime * comp.Speed;
            comp.CooldownTimer -= delta;

            if (comp.CooldownTimer <= 0)
            {
                comp.CooldownTimer = comp.BaseCooldown;
                Cycle((uid, comp));
            }
        }
    }

    /// <summary>
    ///     Crawl one tile away from it's initial position.
    ///     Replicate itself and the prototype designated.
    ///     Delete itself afterwards.
    /// </summary>
    private void Cycle(Entity<HierophantChaserComponent> ent)
    {
        if (!TryComp<MapGridComponent>(Transform(ent).GridUid, out var grid))
            return;

        var gridEnt = ((EntityUid) Transform(ent).GridUid!, (MapGridComponent) grid);

        var pos = Transform(ent).Coordinates.Position;
        var confines = new Box2(pos - Vector2.One, pos + Vector2.One);

        // get random position tiles pool
        var randomPos = _map.GetLocalTilesIntersecting(ent, grid, confines).ToList();
        var deltaPos = Vector2.Zero;

        // if there is a target get it's position delta instead
        if (ent.Comp.Target != null)
            deltaPos = Transform((EntityUid) ent.Comp.Target).Coordinates.Position - pos;

        // if the target is still missing we'll just pick a random tile
        if (deltaPos == Vector2.Zero)
            deltaPos = _random.Pick(randomPos).GridIndices;

        // translate it
        deltaPos = TranslateDelta(deltaPos);

        // generate new position
        var curTile = _map.GetTileRef(gridEnt, _xform.GetMapCoordinates(Transform(ent))).GridIndices;
        var newPos = new MapCoordinates(curTile + deltaPos, Transform(ent).MapID);

        Spawn(ent.Comp.SpawnPrototype, newPos);
        var @this = Spawn(Prototype(ent)!.ID, newPos);

        if (TryComp<HierophantChaserComponent>(@this, out var newChaser))
            newChaser.Steps = ent.Comp.Steps + 1;

        QueueDel(ent);
    }

    private Vector2i TranslateDelta(Vector2 delta)
    {
        int x = 0, y = 0;

        // this is shitty but it works and i'm not complaining
        if (delta.X >= .5f) x = 1;
        if (delta.Y >= .5f) y = 1;
        if (delta.X <= -.5f) x = -1;
        if (delta.Y <= -.5f) y = -1;

        var translated = new Vector2i(x, y);

        return translated;
    }
}
