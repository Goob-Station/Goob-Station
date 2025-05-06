using System.Linq;
using Content.Shared._Shitcode.Heretic.Components;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._Shitcode.Heretic.Systems;

public abstract class SharedContinuousBeamSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ContinuousBeamComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var beam, out var xform))
        {
            var coords = _transform.GetMapCoordinates(uid, xform);

            foreach (var (netTarget, data) in beam.Data)
            {
                data.Lifetime -= frameTime;

                if (data.Lifetime <= 0 || !TryGetEntity(netTarget, out var target) ||
                    !TryComp(target.Value, out TransformComponent? targetXform))
                {
                    if (_net.IsServer)
                        data.Lifetime = 0f; // Mark it for deletion basically
                    continue;
                }

                var targetCoords = _transform.GetMapCoordinates(target.Value, targetXform);

                if (targetCoords.MapId != coords.MapId || (targetCoords.Position - coords.Position).LengthSquared() >
                    data.MaxDistanceSquared)
                {
                    if (_net.IsServer)
                        data.Lifetime = 0f;
                    continue;
                }

                data.Timer -= frameTime;

                if (data.Timer > 0f)
                    continue;

                data.Timer = data.TickInterval;

                if (data.Event == null)
                    continue;

                var ev = data.Event;
                ev.Handled = false;
                ev.User = GetNetEntity(uid);
                ev.Target = netTarget;
                RaiseLocalEvent(uid, (object) ev);
                if (ev.Handled)
                    continue;
                RaiseLocalEvent(target.Value, ev);
            }

            if (_net.IsClient)
                continue;

            beam.Data = beam.Data.Where(x => x.Value.Lifetime > 0f).ToDictionary();

            if (beam.Data.Count == 0)
            {
                RemCompDeferred(uid, beam);
                continue;
            }

            Dirty(uid, beam);
        }
    }
}
