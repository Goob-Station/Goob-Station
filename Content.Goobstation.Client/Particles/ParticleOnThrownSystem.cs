using Content.Goobstation.Shared.Particles;
using Content.Shared.Throwing;

namespace Content.Goobstation.Client.Particles;

public sealed partial class ParticleOnThrownSystem : EntitySystem
{
    [Dependency] private ParticleSystem _particles = default!;

    private readonly Dictionary<EntityUid, ActiveEmitter> _emitters = new();
    private readonly List<EntityUid> _toStop = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ParticleOnThrownComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(Entity<ParticleOnThrownComponent> ent, ref ComponentShutdown args)
    {
        Stop(ent);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        var query = EntityQueryEnumerator<ParticleOnThrownComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var thrown = HasComp<ThrownItemComponent>(uid);
            var active = _emitters.ContainsKey(uid);

            if (thrown && !active && _particles.CreateParticle(comp.Effect, uid, comp.ColorOverride) is { } emitter)
                _emitters[uid] = emitter;
            else if (!thrown && active)
                _toStop.Add(uid);
        }

        foreach (var uid in _toStop)
        {
            Stop(uid);
        }
        _toStop.Clear();
    }

    private void Stop(EntityUid uid)
    {
        if (_emitters.Remove(uid, out var emitter))
            _particles.RemoveParticle(emitter);
    }
}
