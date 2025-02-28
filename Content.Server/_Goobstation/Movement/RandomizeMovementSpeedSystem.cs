using Content.Server._Goobstation.Movement;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;



public sealed partial class RandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifierSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnEquipped);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    public void OnEquipped(EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {

        var initialModifier = _random.NextFloat(RandomizeMovementspeedComponent.LowerBound, RandomizeMovementspeedComponent.UpperBound);
        // Something about being unable to access a non-static field in a static context
        // var walkModifier =
        // var sprintModifier =
        // The idea here is get two floats from the component, one lower and one higher. Generate number between those two numbers.
        // Get the movement speed of the user, multiply by that number, then apply it.
        // Somewhere in here it's supposed to take an interval to randomize every X amount of seconds.
    }

    public void OnUnequipped(EntityUid uid, RandomizeMovementspeedComponent comp, GotUnequippedHandEvent args)
    {
        // Delete component somehow
        args.Handled = true;
    }

}
