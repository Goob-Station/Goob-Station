using Content.Server.Bible;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Movement;

public sealed class RandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    #region Helper Functions
    private void OnGotEquippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotEquippedHandEvent args)
    {
        // Refresh the movement speed modifiers.
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
        // Get the Uid of the entity who picked up the item.
        GetEntityUid(ent, ref args);
    }

    private void OnGotUnequippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotUnequippedHandEvent args)
    {
        // Refresh the movement speed modifiers.
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
        // Reset the user Uid.
        ent.Comp.EntityUid = default!;
    }

    private void GetEntityUid(Entity<RandomizeMovementspeedComponent> ent, ref GotEquippedHandEvent args)
    {
        // Set the entity Uid field of the Component equal to the entity who picked up the item.
        ent.Comp.EntityUid = args.User;
    }

    private float GetMovementSpeedModifiers(RandomizeMovementspeedComponent comp)
    {
        // Generate a modifier, which is a float between the minimum and maxiumum defined by the component.
        var modifier = _random.NextFloat(comp.Min, comp.Max);
        // Return that modifier.
        return modifier;

    }
    private static void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        // Set the variable modifier equal to the components current modifier field.
        var modifier = comp.CurrentModifier;
        // Modify the speed of the entity according to the modifier.
        args.Args.ModifySpeed(modifier, modifier);
    }

    #endregion

    #region Update Loop
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Check if it's time to execute again.
        if (_timing.CurTime < _nextExecutionTime)
            return;

        var query = EntityQueryEnumerator<RandomizeMovementspeedComponent>();
        while (query.MoveNext(out var comp))
        {
            foreach (var ent in EntityQuery<RandomizeMovementspeedComponent>())
            {
                // Check if the user is capable of wielding a nullrod.
                if (!HasComp<BibleUserComponent>(comp.EntityUid))
                    return;

                // Generate the new modifier.
                var modifier = GetMovementSpeedModifiers(comp);

                // Set the new modifier.
                comp.CurrentModifier = modifier;

                // Call the event, and refresh the movement speed modifiers.
                _movementSpeedModifier.RefreshMovementSpeedModifiers(comp.EntityUid);
            }
        }
        // Set the next execution time.
        _nextExecutionTime = _timing.CurTime + ExecutionInterval;
    }

    #endregion


}

