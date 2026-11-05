using Content.Shared._Trauma.Throwing;
using Robust.Client.Physics;
using Robust.Client.Player;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Client._Trauma.Throwing;

/// <summary>
/// Lets thrown items' physics be predicted by whoever threw them.
/// </summary>
public sealed class PredictedThrowingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PredictedThrownItemComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PredictedThrownItemComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PredictedThrownItemComponent, UpdateIsPredictedEvent>(OnUpdateIsPredicted);
        SubscribeLocalEvent<PredictedThrownItemComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnUpdateIsPredicted(Entity<PredictedThrownItemComponent> ent, ref UpdateIsPredictedEvent args)
    {
        // Goob - only the client that threw it predicts it, so everything using PlayPredicted with the thrower works
        if (ent.Comp.LocallyPredicted || ent.Comp.Thrower is {} thrower && thrower == _player.LocalEntity)
            args.IsPredicted = true;
    }

    private void OnStartup(Entity<PredictedThrownItemComponent> ent, ref ComponentStartup args)
    {
        // added by this client predicting a throw, not from a server state
        ent.Comp.LocallyPredicted = !_timing.ApplyingState;

        // start predicted physics immediately
        _physics.UpdateIsPredicted(ent.Owner);
    }

    private void OnHandleState(Entity<PredictedThrownItemComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        // thrower may have changed from being thrown again
        _physics.UpdateIsPredicted(ent.Owner);
    }

    private void OnShutdown(Entity<PredictedThrownItemComponent> ent, ref ComponentShutdown args)
    {
        // stop predicted physics after a brief delay so it doesn't rubber band with client-server ping
        var uid = ent.Owner;
        Timer.Spawn(1000, () =>
        {
            // Goob - the entity can be deleted before the timer fires
            if (!Deleted(uid))
                _physics.UpdateIsPredicted(uid);
        });
    }
}
