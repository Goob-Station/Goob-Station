using Content.Server._Goobstation.Movement;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// This isn't actually functioning yet, but it DOES properly add and remove itself.
// Once one of the smart people of the ivory tower help me I can get this working probably.


public sealed partial class RandomizeMovementSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifierSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(3);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotEquippedHandEvent>(OnItemInHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, GotUnequippedHandEvent>(OnUnequipped);
    }

    public void OnItemInHand(EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        // When the item is equipped, add the component to the player.
        if (args.User != null)
            EnsureComp<RandomizeMovementspeedComponent>(args.User);
    }

    public void Update(float frameTime, EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {

        base.Update(frameTime);

        if (_timing.CurTime < _nextExecutionTime)
            return;

        TryModifySpeed(uid, comp, args);

        _nextExecutionTime = _timing.CurTime + ExecutionInterval;

    }

    public void TryModifySpeed(EntityUid uid, RandomizeMovementspeedComponent comp, GotEquippedHandEvent args)
    {
        Logger.Info("Task executed at " + _timing.CurTime);
        var modifier = _random.NextFloat(comp.Min, comp.Max);

        _speedModifierSystem.ChangeBaseSpeed(args.User, modifier, modifier, modifier);
        _speedModifierSystem.RefreshMovementSpeedModifiers(args.User);
    }

    public void OnUnequipped(EntityUid uid, RandomizeMovementspeedComponent comp, GotUnequippedHandEvent args)
    {
        // Remove component when the item is unequipped.
        RemComp<RandomizeMovementspeedComponent>(args.User);
        // Set to handled to prevent fuckery.
        args.Handled = true;
    }

}
