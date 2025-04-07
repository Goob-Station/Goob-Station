// SPDX-FileCopyrightText: 2025 Rouden <149893554+Roudenn@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Destructible.Thresholds;

namespace Content.Server._Lavaland.Weather.Gamerule;

[RegisterComponent]
public sealed partial class LavalandStormSchedulerRuleComponent : Component
{
    /// <summary>
    ///     How long until the next check for an event runs
    /// </summary>
    [DataField] public float EventClock = 600f; // Ten minutes

    /// <summary>
    ///     How much time it takes in seconds for a lavaland storm to be raised.
    /// </summary>
    [DataField] public MinMax Delays = new(20 * 60, 40 * 60);
}