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
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnItemInHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    public void OnItemInHand(EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        // When the item is equipped, add the component to the player.
        EnsureComp<RandomizeMovementspeedComponent>(args.User);
    }

    public void OnInterval(EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        var speedModifier = _random.NextFloat(comp.Min, comp.Max);
        var interval = comp.Interval;
        // Not finished, trying to get the component to actually apply first.
    }

    public void OnUnequipped(EntityUid uid, RandomizeMovementspeedComponent comp, GotUnequippedHandEvent args)
    {
        // Remove component when the item is unequipped.
        RemComp<RandomizeMovementspeedComponent>(args.User);
        // Set to handled to prevent fuckery.
        args.Handled = true;
    }

}
