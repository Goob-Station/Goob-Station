using Content.Shared._Goobstation.Wizard.Projectiles;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Wizard.Systems;

public sealed partial class TheSwapSystem : EntitySystem
{
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public void Swap(EntityUid uid,
        TransformComponent xform,
        EntityUid otherUid,
        TransformComponent otherXform,
        SoundSpecifier? swapSound,
        EntProtoId swapEffect,
        bool spawnSecondaryEffects = true)
    {
        _pulling.StopAllPulls(uid);
        _pulling.StopAllPulls(otherUid);
        SpawnEffects(uid, xform);
        if (spawnSecondaryEffects)
            SpawnEffects(otherUid, otherXform);
        _xform.SwapPositions((uid, xform), (otherUid, otherXform));
        _physics.WakeBody(uid);
        _physics.WakeBody(otherUid);
        return;

        void SpawnEffects(EntityUid ent, TransformComponent transform)
        {
            if (_net.IsClient)
                return;

            _audio.PlayPvs(swapSound, transform.Coordinates);
            var effect = Spawn(swapEffect, transform.Coordinates);
            if (TryComp(effect, out TrailComponent? trail))
            {
                trail.SpawnPosition = _xform.GetWorldPosition(transform);
                trail.RenderedEntity = ent;
                Dirty(effect, trail);
            }

            _xform.SetParent(effect, Transform(effect), ent, transform);
        }
    }
}