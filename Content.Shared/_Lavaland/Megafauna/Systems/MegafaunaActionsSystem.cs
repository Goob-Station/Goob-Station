using System.Numerics;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.Movement;
using Content.Shared.Actions.Components;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// Handles general actions that are useful for all megafauna bosses.
/// </summary>
public sealed class MegafaunaActionsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsComponent, SpawnEntityActionEvent>(OnAttackAction);
        SubscribeLocalEvent<ActionsComponent, ToggleTileMovementActionEvent>(OnTileMovement);
    }

    private void OnAttackAction(Entity<ActionsComponent> ent, ref SpawnEntityActionEvent args)
    {
        if (_net.IsClient // PredictedSpawn doesn't support spawning entities without initializing them yet...
            || args.Handled
            || _xform.GetGrid(args.Target) == null)
            return;

        EntityUid spawned;
        if (args.Entity != null && args.AttachToTarget)
            spawned = EntityManager.CreateEntityUninitialized(args.Spawn, new EntityCoordinates(args.Entity.Value, Vector2.Zero));
        else if (args.SpawnAtUser)
            spawned = EntityManager.CreateEntityUninitialized(args.Spawn, Transform(args.Performer).Coordinates);
        else
            spawned = EntityManager.CreateEntityUninitialized(args.Spawn, args.Target);

        var ev = new SpawnedByActionEvent(ent.Owner, args.Entity);
        RaiseLocalEvent(spawned, ref ev);

        // We run MapInitEvent only after SpawnedByActionEvent so all values are already set properly.
        EntityManager.InitializeEntity(spawned);
        EntityManager.RunMapInit(spawned, MetaData(spawned)); // InitializeEntity doesn't trigger MapInit event by itself....

        if (args.Entity != null && args.AttachToTarget)
            _xform.SetParent(spawned, args.Entity.Value); // It doesn't work without that for whatever reason??

        args.Handled = true;
    }

    private void OnTileMovement(Entity<ActionsComponent> ent, ref ToggleTileMovementActionEvent args)
    {
        if (args.Handled)
            return;

        if (HasComp<HierophantBeatComponent>(args.Target))
            RemComp<HierophantBeatComponent>(args.Target);
        else
            EnsureComp<HierophantBeatComponent>(args.Target);

        args.Handled = true;
    }
}
