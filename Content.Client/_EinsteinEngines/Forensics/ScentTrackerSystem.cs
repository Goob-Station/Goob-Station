// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 SolsticeOfTheWinter <solsticeofthewinter@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._EinsteinEngines.Forensics.Systems;
using Content.Shared._EinsteinEngines.Forensics;
using Content.Shared.Forensics.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Client.Player;

namespace Content.Client._EinsteinEngines.Forensics;

public sealed class ScentTrackerSystem : SharedScentTrackerSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Goobstation - move trycomp, scent = empty outside of the while loop
        // If the player can't track scents, continuing beyond this point is a waste of processing power.
        if (!TryComp<ScentTrackerComponent>(_playerManager.LocalEntity, out var scentcomp) || scentcomp.Scent == string.Empty)
            return;

        var query = AllEntityQuery<ForensicsComponent>();
        while (query.MoveNext(out var uid, out var comp))
            if (scentcomp.Scent == comp.Scent
                && _timing.CurTime > comp.TargetTime)
            {
                comp.TargetTime = _timing.CurTime + TimeSpan.FromSeconds(1.0f);
                Spawn("ScentTrackEffect", _transform.GetMapCoordinates(uid).Offset(_random.NextVector2(0.25f)));
            }
    }
}