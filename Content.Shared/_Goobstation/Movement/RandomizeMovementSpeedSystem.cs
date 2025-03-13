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

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
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

    public float GetMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent comp)
    {
        var modifier = _random.NextFloat(comp.Min, comp.Max);
        return modifier;

    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _nextExecutionTime)
        {
            Logger.DebugS("RandomizeMovementSpeed", $"Skipping update. Time left: {_nextExecutionTime - _timing.CurTime}");
            return;
        }

        foreach (var ent in EntityQuery<RandomizeMovementspeedComponent>())
        {
            var uid = ent.Owner;
            var comp = ent;
            Logger.DebugS("RandomizeMovementSpeed", $"Processing entity {uid}. CurrentModifier: {comp.CurrentModifier}");

            var modifier = GetMovementSpeedModifiers(uid, comp);
            comp.CurrentModifier = modifier;
            Logger.DebugS("RandomizeMovementSpeed", $"Generated new modifier {modifier} for entity {uid}");

            RaiseLocalEvent(uid, new RefreshMovementSpeedModifiersEvent(), true);
            Logger.DebugS("RandomizeMovementSpeed", $"Raised RefreshMovementSpeedModifiersEvent for entity {uid}");
        }

        _nextExecutionTime = _timing.CurTime + ExecutionInterval;
        Logger.DebugS("RandomizeMovementSpeed", $"Next execution scheduled at {_nextExecutionTime}");
    }



    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        args.Args.ModifySpeed(comp.CurrentModifier, comp.CurrentModifier);
    }

}
