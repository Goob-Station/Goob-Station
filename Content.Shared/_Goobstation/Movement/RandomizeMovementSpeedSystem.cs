using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Movement;

public sealed class RandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    private void OnGotEquippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotEquippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }

    private void OnGotUnequippedHand(Entity<RandomizeMovementspeedComponent> ent, ref GotUnequippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }

    private float GetMovementSpeedModifiers(RandomizeMovementspeedComponent comp)
    {
        var modifier = _random.NextFloat(comp.Min, comp.Max);
        return modifier;

    }
    public override void Update(float frameTime) // Okay so this works, but ONLY when injured. What the fuck dude.
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextExecutionTime)
            return;

        var query = EntityQueryEnumerator<RandomizeMovementspeedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            foreach (var ent in EntityQuery<RandomizeMovementspeedComponent>())
            {
                var modifier = GetMovementSpeedModifiers(comp);
                comp.CurrentModifier = modifier;

                _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
            }
        }
        _nextExecutionTime = _timing.CurTime + ExecutionInterval;
    }


    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        var modifier = comp.CurrentModifier;
        args.Args.ModifySpeed(modifier, modifier);
    }

}

// My best guess is that the Base.Update() in PassiveDamageSystem.cs is somehow activating the event to refresh modifiers.
// Whenever I try to call it manually, it doesn't fucking work.
// I want to hang myself.

