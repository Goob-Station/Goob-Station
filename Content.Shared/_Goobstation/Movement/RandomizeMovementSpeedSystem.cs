using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Movement;

public sealed class HeldSpeedModifierSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

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
    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        var modifier = GetMovementSpeedModifiers(uid, comp);
        args.Args.ModifySpeed(modifier, modifier);
    }
}
