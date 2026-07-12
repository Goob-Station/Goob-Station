using Content.Pirate.Shared.Avali.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Emoting;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Speech;
using Content.Shared.Throwing;

namespace Content.Pirate.Shared.Avali.EntitySystems;

/// <summary>
/// Blocks movement and ordinary interactions while preserving the exit-stasis action.
/// </summary>
public abstract partial class SharedStasisFrozenSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StasisFrozenComponent, UseAttemptEvent>(OnUseAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, PickupAttemptEvent>(OnCancellableAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, ThrowAttemptEvent>(OnCancellableAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, InteractionAttemptEvent>(OnInteractAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<StasisFrozenComponent, ComponentShutdown>(UpdateCanMove);
        SubscribeLocalEvent<StasisFrozenComponent, UpdateCanMoveEvent>(OnUpdateCanMove);
        SubscribeLocalEvent<StasisFrozenComponent, PullAttemptEvent>(OnPullAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, AttackAttemptEvent>(OnCancellableAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, ChangeDirectionAttemptEvent>(OnCancellableAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, EmoteAttemptEvent>(OnEmoteAttempt);
        SubscribeLocalEvent<StasisFrozenComponent, SpeakAttemptEvent>(OnSpeakAttempt);
    }

    private void OnUseAttempt(EntityUid uid, StasisFrozenComponent component, UseAttemptEvent args)
    {
        if (!TryComp<StasisComponent>(uid, out var stasis) ||
            stasis.ExitStasisActionEntity == null ||
            args.Used != stasis.ExitStasisActionEntity)
        {
            args.Cancel();
        }
    }

    private static void OnCancellableAttempt(EntityUid uid, StasisFrozenComponent component,
        CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    private void OnInteractAttempt(Entity<StasisFrozenComponent> ent, ref InteractionAttemptEvent args)
    {
        if (args.Target != null)
            args.Cancelled = true;
    }

    private static void OnPullAttempt(EntityUid uid, StasisFrozenComponent component, PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnStartup(EntityUid uid, StasisFrozenComponent component, ComponentStartup args)
    {
        if (TryComp<PullableComponent>(uid, out var pullable))
        {
            // Pirate: source has no Goob grab-intent release gate, so force the stop.
            _pulling.TryStopPull(uid, pullable, ignoreGrab: true);
        }

        UpdateCanMove(uid, component, args);
    }

    private void OnUpdateCanMove(EntityUid uid, StasisFrozenComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage <= ComponentLifeStage.Running)
            args.Cancel();
    }

    private void UpdateCanMove(EntityUid uid, StasisFrozenComponent component, EntityEventArgs args)
    {
        _blocker.UpdateCanMove(uid);
    }

    private static void OnSpeakAttempt(EntityUid uid, StasisFrozenComponent component, SpeakAttemptEvent args)
    {
        if (component.Muted)
            args.Cancel();
    }

    private static void OnEmoteAttempt(EntityUid uid, StasisFrozenComponent component, EmoteAttemptEvent args)
    {
        if (component.Muted)
            args.Cancel();
    }
}
