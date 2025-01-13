using System.Numerics;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;


namespace Content.Shared.Turnstile;

/// <summary>
/// This handles...
/// </summary>


public enum CardinalDirection
{
    East,
    West,
    North,
    South,
}
public sealed class MovementBlockerSystem : EntitySystem
{
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MovementBlockerComponent,MoveEvent>(OnMove);
        SubscribeLocalEvent<MovementBlockerComponent,UpdateCanMoveEvent>(OnMoveAttempt);

    }

    private void OnMoveAttempt(EntityUid uid, MovementBlockerComponent comp, UpdateCanMoveEvent args)
    {
        Logger.Debug("OnMoveAttempt");
        if (comp.LifeStage > ComponentLifeStage.Running)
            return;

        if (!HasComp<TurnstileComponent>(comp.Turnstile))
            return;

        if (comp.CurrentDir !=
            _entityManager.GetComponent<TurnstileComponent>(comp.Turnstile).TurnstileDirection)
            return;

        args.Cancel(); // no more scurrying around
    }

    CardinalDirection GetCardinalDirection(Vector2 movementVector)
    {
        if (Math.Abs(movementVector.X) > Math.Abs(movementVector.Y))
        {
            return movementVector.X > 0 ? CardinalDirection.East : CardinalDirection.West;
        }

        return movementVector.Y > 0 ? CardinalDirection.North : CardinalDirection.South;
    }

    private void OnMove(EntityUid uid, MovementBlockerComponent component, MoveEvent args)
    {

        var movementVector = args.NewPosition - args.OldPosition;
        var length = (float)Math.Sqrt((movementVector.X * movementVector.X) + (movementVector.Y * movementVector.Y));

        if (length == 0)
            return;

        var normalizedVector = new Vector2(movementVector.X / length, movementVector.Y / length);
        component.CurrentDir = GetCardinalDirection(normalizedVector);
        Logger.Debug(component.CurrentDir.ToString());
        _blocker.UpdateCanMove(uid);

    }

}
