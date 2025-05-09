// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Administration.Components;
using Content.Shared.Administration;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server.Administration.Systems;

public sealed class BufferingSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<BufferingComponent>();
        while (query.MoveNext(out var uid, out var buffering))
        {
            if (buffering.BufferingIcon is not null)
            {
                buffering.BufferingTimer -= frameTime;
                if (!(buffering.BufferingTimer <= 0.0f))
                    continue;

                Del(buffering.BufferingIcon.Value);
                RemComp<AdminFrozenComponent>(uid);
                buffering.TimeTilNextBuffer = _random.NextFloat(buffering.MinimumTimeTilNextBuffer, buffering.MaximumTimeTilNextBuffer);
                buffering.BufferingIcon = null;
            }
            else
            {
                buffering.TimeTilNextBuffer -= frameTime;
                if (!(buffering.TimeTilNextBuffer <= 0.0f))
                    continue;

                buffering.BufferingTimer = _random.NextFloat(buffering.MinimumBufferTime, buffering.MaximumBufferTime);
                buffering.BufferingIcon = Spawn("BufferingIcon", new EntityCoordinates(uid, Vector2.Zero));
                EnsureComp<AdminFrozenComponent>(uid);
            }
        }
    }
}