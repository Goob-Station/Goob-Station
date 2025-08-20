using Content.Goobstation.Common.Bingle;
using Content.Goobstation.Shared.Bingle;
using Content.Server.Actions;
using Content.Server.Mind;
using Content.Server.Power.EntitySystems;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.Bingle;

public sealed partial class BingleSystem : EntitySystem
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;

    private void InitializePlacer()
    {
        SubscribeLocalEvent<BinglePrimePlacerComponent, MapInitEvent>(OnStartup);
        SubscribeLocalEvent<BinglePrimePlacerComponent, PlaceBinglePitActionEvent>(OnPlaceBinglePrime);
    }

    private void OnStartup(Entity<BinglePrimePlacerComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ent.Comp.PlaceActionPrototype);
    }

    private void OnPlaceBinglePrime(Entity<BinglePrimePlacerComponent> ent, ref PlaceBinglePitActionEvent args)
    {
        var xform = Transform(ent);

        if (!CanPlacePrimeHere((ent.Owner, xform)) || !_mind.TryGetMind(ent, out var mindId, out var mindComp))
            return;

        var prime = Spawn(ent.Comp.BinglePrimePrototype, xform.Coordinates);

        _mind.TransferTo(mindId, prime, false, true, mindComp);

        QueueDel(ent.Owner);
    }

    private bool CanPlacePrimeHere(Entity<TransformComponent> ent)
    {
        return !_lookup.AnyEntitiesInRange(ent, 1, LookupFlags.Static)
            && ent.Comp.GridUid is { } gridUid
            && TryComp(gridUid, out MapGridComponent? mapGrid)
            && !_map.GetTileRef((gridUid, mapGrid), ent.Comp.Coordinates).Tile.IsEmpty;
    }
}