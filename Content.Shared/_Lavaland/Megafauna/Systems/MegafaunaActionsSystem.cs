using System.Numerics;
using Content.Shared._Lavaland.Megafauna.Events;
using Content.Shared._Lavaland.Movement;
using Content.Shared.Actions.Components;
using Robust.Shared.Map;

namespace Content.Shared._Lavaland.Megafauna.Systems;

/// <summary>
/// Handles general actions that are useful for all megafauna bosses.
/// </summary>
public sealed class MegafaunaActionsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActionsComponent, SpawnEntityActionEvent>(OnAttackAction);
        SubscribeLocalEvent<ActionsComponent, ToggleTileMovementActionEvent>(OnTileMovement);
    }

    private void OnAttackAction(Entity<ActionsComponent> ent, ref SpawnEntityActionEvent args)
    {
        if (args.Handled
            || _xform.GetGrid(args.Target) == null)
            return;

        EntityUid spawned;
        if (args.Entity != null
            && args.AttachToTarget)
            spawned = PredictedSpawnAttachedTo(args.Spawn, new EntityCoordinates(args.Entity.Value, Vector2.Zero));
        else if (args.SpawnAtUser)
            spawned = PredictedSpawnAtPosition(args.Spawn, Transform(args.Performer).Coordinates);
        else
            spawned = PredictedSpawnAtPosition(args.Spawn, args.Target);

        var ev = new SpawnedByActionEvent(ent.Owner, args.Entity);
        RaiseLocalEvent(spawned, ref ev);
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
