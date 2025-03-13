using Content.Shared.Damage;
using Content.Shared.Hands;
using Content.Shared.Movement.Events;
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

    private void OnMoveInput(Entity<RandomizeMovementspeedComponent> ent, ref MoveInputEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.Entity);

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
            return;

        foreach (var ent in EntityQuery<RandomizeMovementspeedComponent>())
        {
            var uid = ent.Owner;
            var comp = ent;

            var modifier = GetMovementSpeedModifiers(uid, comp);
            comp.CurrentModifier = modifier;

            RaiseLocalEvent(uid, new RefreshMovementSpeedModifiersEvent(), true);
            _damageableSystem.TryChangeDamage(uid, comp.DummyDamage, false, origin: uid);
        }

        _nextExecutionTime = _timing.CurTime + ExecutionInterval;
    }

    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        var modifier = comp.CurrentModifier;
        args.Args.ModifySpeed(modifier, modifier);
    }

}
