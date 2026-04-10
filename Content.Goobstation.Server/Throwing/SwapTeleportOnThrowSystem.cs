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
        SubscribeLocalEvent<SwapTeleportOnThrowComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnThrowHit(Entity<SwapTeleportOnThrowComponent> ent, ref ThrowDoHitEvent args)
    {
        if (args.Handled)
            return;

        var thrower = args.Component.Thrower;
        var target = args.Target;

        if (thrower == null
            || !HasComp<MobStateComponent>(target))
            return;

        var throwerTransform = Transform(thrower.Value);
        var targetTransform = Transform(target);

        var throwerPos = throwerTransform.Coordinates;
        var targetPos = targetTransform.Coordinates;

        var throwerParent = throwerTransform.ParentUid;
        var targetParent = throwerTransform.ParentUid;

        _transform.SetCoordinates(thrower.Value, targetPos);
        _transform.SetCoordinates(target, throwerPos);

        if (!HasComp<MapGridComponent>(targetParent))
            _transform.SetParent(thrower.Value, throwerParent);

        if (!HasComp<MapGridComponent>(throwerParent))
            _transform.SetParent(target, targetParent);

        _audio.PlayPvs(ent.Comp.OriginSound, throwerPos);
        _audio.PlayPvs(ent.Comp.TargetSound, targetPos);

        PredictedQueueDel(ent.Owner);
        args.Handled = true;
    }
}
