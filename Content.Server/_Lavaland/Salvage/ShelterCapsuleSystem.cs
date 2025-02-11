using System.Linq;
using Content.Server._Lavaland.Procedural.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GridPreloader;
using Content.Server.Popups;
using Content.Shared._Lavaland.Salvage;
using Content.Shared.Chemistry.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Lavaland.Salvage;

public sealed class ShelterCapsuleSystem : SharedShelterCapsuleSystem
{
    [Dependency] private readonly GridPreloaderSystem _preloader = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
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
        TryDeployShelterCapsule((uid, component));
    }

    public bool TryDeployShelterCapsule(Entity<ShelterCapsuleComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return false;

        var xform = Transform(ent);
        var comp = ent.Comp;

        // Works only on planets!
        if (xform.GridUid == null || xform.MapUid == null || xform.GridUid != xform.MapUid || !TryComp<MapGridComponent>(xform.GridUid.Value, out var gridComp))
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-planet"), xform.Coordinates);
            return false;
        }

        var proto = _protoMan.Index(comp.PreloadedGrid);
        var worldPos = _transform.GetMapCoordinates(ent, xform);

        // Make sure that surrounding area does not have any entities with physics
        var box = Box2.CenteredAround((worldPos.Position + comp.Offset).Rounded(), comp.BoxSize);
        if (_mapSystem.GetAnchoredEntities(xform.GridUid.Value, gridComp, box).Any())
        {
            _popup.PopupCoordinates(Loc.GetString("shelter-capsule-fail-no-space"), xform.Coordinates);
            return false;
        }

        // Load and place shelter
        var path = proto.Path.CanonPath;
        var mapEnt = xform.MapUid.Value;
        var posFixed = new MapCoordinates(worldPos.Position.Rounded(), worldPos.MapId);

        // Smoke
        var foamEnt = Spawn("Smoke", posFixed);
        var spreadAmount = (int) Math.Round(comp.BoxSize.Length());
        _smoke.StartSmoke(foamEnt, new Solution(), 10f, spreadAmount);

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

            _transform.SetCoordinates(shelter.Value,
                Transform(shelter.Value),
                new EntityCoordinates(mapEnt, worldPos.Position),
                Angle.Zero);
            EnsureComp<LavalandMemberComponent>(shelter.Value);
            _mapMan.DeleteMap(dummyMap);
            return true;
        }

        //_transform.SetParent(shelter.Value, xform.MapUid.Value);
        _transform.SetCoordinates(shelter.Value,
            Transform(shelter.Value),
            new EntityCoordinates(mapEnt, worldPos.Position),
            Angle.Zero);
        EnsureComp<LavalandMemberComponent>(shelter.Value);
        return true;
    }
}
