using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity whenever it has a movement input change.
/// </summary>
[ByRefEvent]
public readonly struct MoveInputEvent
{
    public readonly Entity<InputMoverComponent> Entity;
    public readonly MoveButtons OldMovement;
    public readonly Direction Dir;   //Goobstation - Ventcrawler
    public readonly bool State;  //Goobstation - Ventcrawler

    public bool HasDirectionalMovement => (Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection) != MoveButtons.None;

    public MoveInputEvent(Entity<InputMoverComponent> entity, MoveButtons oldMovement, Direction dir, bool state) //Goobstation - Ventcrawler
    {
        Entity = entity;
        OldMovement = oldMovement;
        Dir = dir;
        State = state;
    }
}
