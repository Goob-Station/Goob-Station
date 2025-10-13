using Content.Shared._EinsteinEngines.Power.Systems;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;

namespace Content.Shared._EinsteinEngines.Silicon.Death;

// Goobstation - Energycrit: Split SiliconChargeDeathSystem into shared.
/// <summary>
///     Blocks discharged silicons from interacting with their environments
///     until they recharge.
/// </summary>
/// <remarks>
///     This is horrible.
/// </remarks>
public abstract class SharedSiliconDeathSystem : EntitySystem
{
    [Dependency] private readonly SharedBatteryDrinkerSystem _drinker = default!;

    public override void Initialize()
    {
        // Disable grabbing and putting things down
        SubscribeLocalEvent<SiliconDownOnDeadComponent, PickupAttemptEvent>(OnPickupAttempt);
        SubscribeLocalEvent<SiliconDownOnDeadComponent, DropAttemptEvent>(OnDropAttempt);

        // Disable interactions on items without a Drink verb
        SubscribeLocalEvent<SiliconDownOnDeadComponent, InteractionAttemptEvent>(OnInteractionAttempt);

        // Prevent dropping items on the ground trying to unequip them
        SubscribeLocalEvent<SiliconDownOnDeadComponent, IsUnequippingAttemptEvent>(OnUnequipAttempt);
    }

    private void OnPickupAttempt(Entity<SiliconDownOnDeadComponent> ent, ref PickupAttemptEvent args)
    {
        if (ent.Comp.Dead)
            args.Cancel();
    }

    private void OnDropAttempt(Entity<SiliconDownOnDeadComponent> ent, ref DropAttemptEvent args)
    {
        if (ent.Comp.Dead)
            args.Cancel();
    }

    private void OnInteractionAttempt(Entity<SiliconDownOnDeadComponent> ent, ref InteractionAttemptEvent args)
    {
        // Discard all verbs on any entities that don't have a drinkable battery
        // anything that slips through the cracks should be prevented by discharged
        // silicons not having ComplexInteractionComponent
        if (ent.Comp.Dead)
            args.Cancelled = !args.Target.HasValue || !_drinker.SearchForSource(args.Target.Value, out _);
    }

    private void OnUnequipAttempt(Entity<SiliconDownOnDeadComponent> ent, ref IsUnequippingAttemptEvent args)
    {
        if (ent.Comp.Dead)
            args.Cancel();
    }
}
