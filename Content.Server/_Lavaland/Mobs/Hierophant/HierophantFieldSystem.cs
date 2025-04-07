using System.Linq;
using System.Threading.Tasks;
using Content.Server._Lavaland.Mobs.Hierophant.Components;
using Robust.Shared.Map.Components;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace Content.Server._Lavaland.Mobs.Hierophant;

public sealed class HierophantFieldSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HierophantFieldGeneratorComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<HierophantFieldGeneratorComponent, EntityTerminatingEvent>(OnTerminating);
    }

    #region Event Handling

    private void OnMapInit(Entity<HierophantFieldGeneratorComponent> ent, ref MapInitEvent args)
    {
        var xform = Transform(ent).Coordinates;
        var hierophant = Spawn(ent.Comp.HierophantPrototype, xform);

        if (!TryComp<HierophantBossComponent>(hierophant, out var hieroComp))
            return;

        ent.Comp.ConnectedHierophant = hierophant;
        hieroComp.ConnectedFieldGenerator = ent;
    }

    private void OnTerminating(Entity<HierophantFieldGeneratorComponent> ent, ref EntityTerminatingEvent args)
    {
        if (ent.Comp.ConnectedHierophant != null &&
            TryComp<HierophantBossComponent>(ent.Comp.ConnectedHierophant.Value, out var hieroComp))
            hieroComp.ConnectedFieldGenerator = null;

        DeleteHierophantFieldImmediatly(ent);
    }

    #endregion

    public void ActivateField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        if (ent.Comp.Enabled)
            return; // how?

        SpawnHierophantField(ent);
        ent.Comp.Enabled = true;
    }

    public void DeactivateField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        if (!ent.Comp.Enabled)
            return; // how?

        DeleteHierophantField(ent);
        ent.Comp.Enabled = false;
    }

    public void DeleteHierophantFieldImmediatly(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
        {
            QueueDel(wall);
        }
    }

    private async Task SpawnHierophantField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var xform = Transform(ent);

        if (!TryComp<MapGridComponent>(xform.GridUid, out var grid))
            return;

        var gridEnt = (xform.GridUid.Value, grid);
        var range = ent.Comp.Radius;
        var center = xform.Coordinates.Position;

        // get tile position of our entity
        if (!_transform.TryGetGridTilePosition((ent, xform), out var tilePos))
            return;

        var pos = _map.TileCenterToVector(gridEnt, tilePos);
        var confines = new Box2(center, center).Enlarged(ent.Comp.Radius);
        var box = _map.GetLocalTilesIntersecting(ent, grid, confines).ToList();

        var confinesS = new Box2(pos, pos).Enlarged(Math.Max(range - 1, 0));
        var boxS = _map.GetLocalTilesIntersecting(ent, grid, confinesS).ToList();
        box = box.Where(b => !boxS.Contains(b)).ToList();

        // fill the box
        foreach (var tile in box)
        {
            var wall = Spawn(ent.Comp.WallPrototype, _map.GridTileToWorld(xform.GridUid.Value, grid, tile.GridIndices));
            ent.Comp.Walls.Add(wall);
        }
    }

    private async Task DeleteHierophantField(Entity<HierophantFieldGeneratorComponent> ent)
    {
        var walls = ent.Comp.Walls.Where(x => !TerminatingOrDeleted(x));
        foreach (var wall in walls)
        {
            QueueDel(wall);
        }
    }
}
