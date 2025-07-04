// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clock;
using Robust.Client.GameObjects;

namespace Content.Client.Clock;

public sealed class ClockSystem : SharedClockSystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ClockComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            if (!sprite.LayerMapTryGet(ClockVisualLayers.HourHand, out var hourLayer) ||
                !sprite.LayerMapTryGet(ClockVisualLayers.MinuteHand, out var minuteLayer))
                continue;

            var time = GetClockTime((uid, comp));
            var hourState = $"{comp.HoursBase}{time.Hours % 12}";
            var minuteState = $"{comp.MinutesBase}{time.Minutes / 5}";
            sprite.LayerSetState(hourLayer, hourState);
            sprite.LayerSetState(minuteLayer, minuteState);
        }
    }
}