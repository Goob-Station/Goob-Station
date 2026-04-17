using Content.Goobstation.Shared.Throwing;
using Content.Shared.Mobs.Components;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;

namespace Content.Goobstation.Server.Throwing;

// This event is predicted incorrectly on the client because of physics,
// that's why for now it stays only on server.
public sealed class SwapTeleportOnThrowSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SwapTeleportOnThrowComponent, ThrowAttemptEvent>(OnThrowHit);
    }

    private void OnThrowHit(Entity<SwapTeleportOnThrowComponent> ent, ref ThrowAttemptEvent args)
    {
        if (args.Cancelled
            || args.TargetUid == null)
            return;

        var thrower = args.Uid;
        var target = args.TargetUid;

        if (!HasComp<MobStateComponent>(target))
            return;

        var throwerTransform = Transform(thrower);
        var targetTransform = Transform(target.Value);

        var throwerPos = throwerTransform.Coordinates;
        var targetPos = targetTransform.Coordinates;

        var throwerParent = throwerTransform.ParentUid;
        var targetParent = throwerTransform.ParentUid;

        _transform.SetCoordinates(thrower, targetPos);
        _transform.SetCoordinates(target.Value, throwerPos);

        if (!HasComp<MapGridComponent>(targetParent))
            _transform.SetParent(thrower, throwerParent);

        if (!HasComp<MapGridComponent>(throwerParent))
            _transform.SetParent(target.Value, targetParent);

        _audio.PlayPvs(ent.Comp.OriginSound, throwerPos);
        _audio.PlayPvs(ent.Comp.TargetSound, targetPos);

        PredictedQueueDel(ent.Owner);
        args.Cancel();
    }
}
