using System.Linq;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GridPreloader;
using Content.Shared._Lavaland.Shelter;
using Content.Shared.Chemistry.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.Salvage;

public sealed class ShelterCapsuleSystem : SharedShelterCapsuleSystem
{
    [Dependency] private readonly GridPreloaderSystem _preloader = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShelterCapsuleComponent, ShelterCapsuleDeployDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(EntityUid uid, ShelterCapsuleComponent component, ShelterCapsuleDeployDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = TryDeployShelterCapsule((uid, component));

        if (args.Handled)
            QueueDel(uid);
    }

    public bool TryDeployShelterCapsule(Entity<ShelterCapsuleComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return false;

        var xform = Transform(ent);
        var comp = ent.Comp;
        var proto = _protoMan.Index(comp.PreloadedGrid);
        var worldPos = _transform.GetMapCoordinates(ent, xform);

        if (!CheckCanDeploy(ent) || xform.MapUid == null)
            return false;

        // Load and place shelter
        var path = proto.Path.CanonPath;
        var mapEnt = xform.MapUid.Value;
        var posFixed = new MapCoordinates((worldPos.Position + comp.Offset).Rounded(), worldPos.MapId);

        // Smoke
        var foamEnt = Spawn("Smoke", worldPos);
        var spreadAmount = (int) Math.Round(comp.BoxSize.Length() * 2);
        _smoke.StartSmoke(foamEnt, new Solution(), comp.DeployTime + 2f, spreadAmount);

        if (!_preloader.TryGetPreloadedGrid(comp.PreloadedGrid, out var shelter))
        {
            _mapSystem.CreateMap(out var dummyMap);
            if (!_mapLoader.TryLoad(dummyMap, path, out var roots) || roots.Count != 1)
            {
                Log.Error("Failed to load Shelter grid properly on it's deployment.");
                return false;
            }

            var shelters = _mapMan.GetAllGrids(dummyMap);
            shelter = shelters.FirstOrDefault(x => !TerminatingOrDeleted(x));

            SetupShelter(shelter.Value, new EntityCoordinates(mapEnt, posFixed.Position));
            _mapMan.DeleteMap(dummyMap);
            return true;
        }

        SetupShelter(shelter.Value, new EntityCoordinates(mapEnt, posFixed.Position));
        return true;
    }

    private void SetupShelter(Entity<TransformComponent?> shelter, EntityCoordinates coords)
    {
        if (!Resolve(shelter, ref shelter.Comp))
            return;

        _transform.SetCoordinates(shelter,
            shelter.Comp,
            coords,
            Angle.Zero);
        EnsureComp<LavalandMemberComponent>(shelter);
    }
}
