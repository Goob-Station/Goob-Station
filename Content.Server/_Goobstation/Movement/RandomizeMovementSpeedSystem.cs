using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

// This isn't actually functioning yet, but it DOES properly add and remove itself.
// Once one of the smart people of the ivory tower help me I can get this working probably.


namespace Content.Server._Goobstation.Movement;

public sealed partial class RandomizeMovementSpeedSystem : EntitySystem
{
    private static readonly TimeSpan ExecutionInterval = TimeSpan.FromSeconds(2);
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifierSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _nextExecutionTime = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RandomizeMovementspeedComponent, EquippedHandEvent>(OnItemInHand);
        SubscribeLocalEvent<RandomizeMovementspeedComponent, UnequippedHandEvent>(OnUnequipped);
    }

    public void OnItemInHand(EntityUid uid,
        RandomizeMovementspeedComponent comp,
        EquippedHandEvent args)
    {
        EnsureComp<RandomizeMovementspeedComponent>(args.User);
    }
    public void TryModifySpeed(EntityUid uid,
        float modifier,
        RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(modifier);
        _speedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    public void OnUnequipped(EntityUid uid,
        RandomizeMovementspeedComponent comp,
        UnequippedHandEvent args)
    {
        RemComp<RandomizeMovementspeedComponent>(args.User);
        args.Handled = true;
    }
}
