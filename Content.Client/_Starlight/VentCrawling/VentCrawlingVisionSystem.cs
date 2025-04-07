// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2025 Rinary <72972221+Rinary1@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 ss14-Starlight <ss14-Starlight@outlook.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.SubFloor;
using Content.Shared._Starlight.VentCrawling;
using Robust.Client.Player;
using Robust.Shared.Timing;

namespace Content.Client._Starlight.VentCrawling;

public sealed class VentCrawlingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly SubFloorHideSystem _subFloorHideSystem = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var player = _player.LocalPlayer?.ControlledEntity;

        var ventCrawlerQuery = GetEntityQuery<VentCrawlerComponent>();

        if (!ventCrawlerQuery.TryGetComponent(player, out var playerVentCrawlerComponent))
            return;

        _subFloorHideSystem.ShowVentPipe = playerVentCrawlerComponent.InTube;
    }
}