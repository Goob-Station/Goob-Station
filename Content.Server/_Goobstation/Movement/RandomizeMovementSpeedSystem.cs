using Content.Server._Goobstation.Movement;
using Content.Shared.Hands;
using Content.Shared.Movement.Components;
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
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, EquippedHandEvent>(OnItemInHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, UnequippedHandEvent>(OnUnequipped);
    }

    public void OnItemInHand(EntityUid uid, RandomizeMovementspeedComponent comp, EquippedHandEvent args)
    {
        // When the item is equipped, add the component to the player.
        EnsureComp<RandomizeMovementspeedComponent>(uid);
    }

    public void Update(float frameTime, EntityUid uid, RandomizeMovementspeedComponent comp, EquippedHandEvent args)
    {

        base.Update(frameTime);

        if (_timing.CurTime < _nextExecutionTime)
            return;

        var modifier = _random.NextFloat(comp.Min, comp.Max);
        _nextExecutionTime = _timing.CurTime + ExecutionInterval;

        _speedModifierSystem.ChangeBaseSpeed(uid, modifier, modifier, modifier);
        _speedModifierSystem.RefreshMovementSpeedModifiers(uid);

    }

    public void OnUnequipped(EntityUid uid, RandomizeMovementspeedComponent comp, UnequippedHandEvent args)
    {
        RemCompDeferred<RandomizeMovementspeedComponent>(args.User);
        args.Handled = true;
    }

}
