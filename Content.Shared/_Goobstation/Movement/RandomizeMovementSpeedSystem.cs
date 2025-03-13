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
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, MapInitEvent>(OnPendingMapInit);
    }
    private void OnPendingMapInit(EntityUid uid, RandomizeMovementspeedComponent component, MapInitEvent args)
    {
        component.NextRandomize = _timing.CurTime + TimeSpan.FromSeconds(3f);
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
        var curTime = _timing.CurTime;

        foreach (var ent in EntityQuery<RandomizeMovementspeedComponent>())
        {
            if (curTime < ent.NextRandomize)
                continue;

            var uid = ent.Owner;
            var comp  = ent;

            var modifier = GetMovementSpeedModifiers(uid, comp);
            comp.CurrentModifier = modifier;

            RaiseLocalEvent(uid, new RefreshMovementSpeedModifiersEvent(), true);

            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }



    private void OnRefreshMovementSpeedModifiers(EntityUid uid, RandomizeMovementspeedComponent  comp, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        var modifier = comp.CurrentModifier;
        args.Args.ModifySpeed(modifier, modifier);
    }

}
