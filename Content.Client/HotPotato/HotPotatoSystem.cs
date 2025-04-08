// SPDX-FileCopyrightText: 2023 AJCM <AJCM@tutanota.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.HotPotato;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Client.HotPotato;

public sealed class HotPotatoSystem : SharedHotPotatoSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = AllEntityQuery<ActiveHotPotatoComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.TargetTime)
                continue;
            comp.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(comp.EffectCooldown);
            Spawn("HotPotatoEffect", _transform.GetMapCoordinates(uid).Offset(_random.NextVector2(0.25f)));
        }
    }
}