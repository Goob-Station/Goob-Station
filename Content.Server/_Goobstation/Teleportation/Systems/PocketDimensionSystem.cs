using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Teleportation.Components;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Teleportation;

/// <summary>
/// This handles pocket dimensions and their portals.
/// </summary>
public sealed class PocketDimensionSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private ISawmill _sawmill = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PocketDimensionComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<PocketDimensionComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);

        _sawmill = Logger.GetSawmill("pocket_dimension");
    }

    private void OnRemoved(EntityUid uid, PocketDimensionComponent comp, ComponentRemove args)
    {
        if (!Deleted(comp.PocketDimensionMap))
            QueueDel(comp.PocketDimensionMap.Value);
    }

    private void OnGetVerbs(EntityUid uid, PocketDimensionComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !HasComp<HandsComponent>(args.User))
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("pocket-dimension-verb-text"),
            Act = () => HandleActivation(uid, comp, args.User)
        };
        args.Verbs.Add(verb);
    }

    /// <summary>
    /// Handles toggling the portal to the pocket dimension.
    /// </summary>
    private void HandleActivation(EntityUid uid, PocketDimensionComponent comp, EntityUid user)
    {
        if (Deleted(comp.PocketDimensionMap))
        {
            var map = _mapMan.CreateMap();

            if (!_mapLoader.TryLoad(map, comp.PocketDimensionPath.ToString(), out var roots))
            {
                _sawmill.Error($"Failed to load pocket dimension map {comp.PocketDimensionPath}");
                QueueDel(_mapMan.GetMapEntityId(map));
                return;
            }

            comp.PocketDimensionMap = _mapMan.GetMapEntityId(map);

            // find the pocket dimension's first grid and put the portal there
            bool foundGrid = false;
            foreach (var root in roots)
            {
                if (!HasComp<MapGridComponent>(root))
                    continue;

                // spawn the permanent portal into the pocket dimension, now ready to be used
                var pos = new EntityCoordinates(root, 0, 0);
                comp.ExitPortal = Spawn(comp.ExitPortalPrototype, pos);
                EnsureComp<PortalComponent>(comp.ExitPortal!.Value, out var portal);
                // the TryUnlink cleanup when first trying to create portal will fail without this
                EnsureComp<LinkedEntityComponent>(uid);
                portal.CanTeleportToOtherMaps = true;

                _sawmill.Info($"Created pocket dimension on grid {root} of map {map}");

                // if someone closes your portal you can use the one inside to escape
                _link.OneWayLink(comp.ExitPortal.Value, uid);
                foundGrid = true;
                break;
            }
            if (!foundGrid)
            {
                _sawmill.Error($"Pocket dimension {comp.PocketDimensionPath} had no grids!");
                QueueDel(comp.PocketDimensionMap);
                return;
            }
        }

        var dimension = comp.ExitPortal!.Value;
        if (comp.PortalEnabled)
        {
            // unlink us
            _link.TryUnlink(dimension, uid);
            comp.PortalEnabled = false;
            _audio.PlayPvs(comp.ClosePortalSound, uid);

            // if you are stuck inside the pocket dimension you can use the internal portal to escape
            _link.OneWayLink(dimension, uid);
        }
        else
        {
            // cleanup
            _link.TryUnlink(dimension, uid);
            // link us to the pocket dimension
            _link.TryLink(dimension, uid);
            comp.PortalEnabled = true;
            _audio.PlayPvs(comp.OpenPortalSound, uid);
        }
    }
}
