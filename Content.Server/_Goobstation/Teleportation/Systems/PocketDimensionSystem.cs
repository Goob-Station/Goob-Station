using Content.Shared.Gravity;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Teleportation;
using Content.Shared.Verbs;
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
    [Dependency] private readonly SharedPortalSystem _portal = default!;

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
        if (comp.PocketDimensionMap != null)
        {
            // everything inside will be destroyed so this better be indestructible
            QueueDel(comp.PocketDimensionMap.Value);
        }

        if (comp.EntryPortal != null)
            QueueDel(comp.EntryPortal.Value);
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
        if (comp.PocketDimensionMap == null)
        {
            var map = _mapMan.CreateMap();
            if (!_mapLoader.TryLoad(map, comp.PocketDimensionPath.ToString(), out var roots))
            {
                _sawmill.Error($"Failed to load pocket dimension map {comp.PocketDimensionPath}");
                QueueDel(uid);
                return;
            }

            comp.PocketDimensionMap = _mapMan.GetMapEntityId(map);
            if (TryComp<GravityComponent>(comp.PocketDimensionMap, out var gravity))
                gravity.Enabled = true;

            // find the pocket dimension's first grid and put the portal there
            bool foundGrid = false;
            foreach (var root in roots)
            {
                if (!HasComp<MapGridComponent>(root))
                    continue;

                // spawn the permanent portal into the pocket dimension, now ready to be used
                var pos = Transform(root).Coordinates;
                comp.ExitPortal = Spawn(comp.ExitPortalPrototype, pos);
                _sawmill.Info($"Created pocket dimension on grid {root} of map {map}");

                // if someone closes your portal you can use the one inside to escape
                _link.TryLink(uid, comp.ExitPortal.Value);
                foundGrid = true;
            }
            if (!foundGrid)
            {
                _sawmill.Error($"Pocket dimension {comp.PocketDimensionPath} had no grids!");
                QueueDel(uid);
                return;
            }
        }

        var dimension = comp.ExitPortal!.Value;
        if (comp.EntryPortal != null)
        {
            // portal already exists so unlink and delete it
            _link.TryUnlink(dimension, comp.EntryPortal.Value);
            QueueDel(comp.EntryPortal.Value);
            comp.EntryPortal = null;
            _audio.PlayPvs(comp.ClosePortalSound, uid);

            // if you are stuck inside the pocket dimension you can use the internal portal to escape
            _link.TryLink(uid, dimension);
        }
        else
        {
            // create a portal and link it to the pocket dimension
            comp.EntryPortal = Spawn(comp.EntryPortalPrototype, Transform(uid).Coordinates);
            _link.TryLink(dimension, comp.EntryPortal.Value);
            _transform.SetParent(comp.EntryPortal.Value, uid);
            // make sure the pot can't go through its own portal
            _portal.AddBlacklist(comp.EntryPortal.Value, uid);
            _audio.PlayPvs(comp.OpenPortalSound, uid);

            _link.TryUnlink(uid, dimension);
        }
    }
}
