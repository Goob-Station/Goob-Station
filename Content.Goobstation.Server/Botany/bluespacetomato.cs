using Content.Shared.Mobs.Components;
using Robust.Shared.Audio.Systems;
using Robust.Server.GameObjects;
using Content.Shared.Throwing;

public sealed class BlueSpaceTomatoSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BlueSpaceTomatoComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnThrowHit(EntityUid uid, BlueSpaceTomatoComponent component, ThrowDoHitEvent args)
    {
        if (Deleted(uid))
            return;

        var thrower = args.Component.Thrower;
        var target = args.Target;

        if (!thrower.HasValue || !HasComp<MobStateComponent>(target) || Deleted(thrower.Value))
            return;

        var throwerTransform = Transform(thrower.Value);
        var targetTransform = Transform(target);

        var throwerPos = throwerTransform.Coordinates;
        var targetPos = targetTransform.Coordinates;

        _transform.SetCoordinates(thrower.Value, targetPos);
        _transform.SetCoordinates(target, throwerPos);

        _audio.PlayPvs("/Audio/Effects/teleport_departure.ogg", throwerPos);
        _audio.PlayPvs("/Audio/Effects/teleport_arrival.ogg", targetPos);

        QueueDel(uid);
    }
}

[RegisterComponent]
public sealed partial class BlueSpaceTomatoComponent : Component
{
}
