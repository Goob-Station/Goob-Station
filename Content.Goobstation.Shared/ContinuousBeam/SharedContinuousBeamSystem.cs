// SPDX-FileCopyrightText: 2025 Aviu00 <aviu00@protonmail.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 gluesniffler <linebarrelerenthusiast@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Robust.Shared.Network;
using Content.Goobstation.Common.ContinuousBeam;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.ContinuousBeam;

public abstract class SharedContinuousBeamSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

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
                Logger.Debug($"Lifetime is {data.Lifetime}");

                if (data.Lifetime <= 0
                    || !TryGetEntity(netTarget, out var target)
                    || !_xformQuery.TryComp(target.Value, out var targetXform))
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
