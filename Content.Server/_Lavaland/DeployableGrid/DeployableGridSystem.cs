// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Fluids.EntitySystems;
using Content.Server.GridPreloader;
using Content.Shared._Lavaland.DeployableGrid;
using Content.Shared.Chemistry.Components;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Lavaland.DeployableGrid;

public sealed class DeployableGridSystem : SharedDeployableGridSystem
{
    [Dependency] private readonly GridPreloaderSystem _preloader = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeployableGridComponent, DeployableGridDoAfterEvent>(OnDoAfter);
    }

    private void OnDoAfter(Entity<DeployableGridComponent> ent, ref DeployableGridDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = TryDeployShelterCapsule(ent);

        if (args.Handled)
            QueueDel(ent);
    }

    public bool TryDeployShelterCapsule(Entity<DeployableGridComponent> ent)
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
        var path = proto.Path;
        var mapEnt = xform.MapUid.Value;
        var posFixed = new MapCoordinates((worldPos.Position + comp.Offset).Rounded(), worldPos.MapId);

        // Smoke
        var foamEnt = Spawn("Smoke", worldPos);
        var spreadAmount = (int) Math.Round(comp.BoxSize.Length() * 2);
        _smoke.StartSmoke(foamEnt, new Solution(), comp.DeployTime + 2f, spreadAmount);

        if (!_preloader.TryGetPreloadedGrid(comp.PreloadedGrid, out var shelter))
        {
            _mapSystem.CreateMap(out var dummyMap);
            if (!_mapLoader.TryLoadGrid(dummyMap, path, out var shelterEnt))
            {
                Log.Error("Failed to load Shelter grid properly on it's deployment.");
                return false;
            }

            SetupShelter(shelterEnt.Value.Owner, new EntityCoordinates(mapEnt, posFixed.Position));
            _mapSystem.DeleteMap(dummyMap);
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

        // TODO flattening of the area
    }
}
