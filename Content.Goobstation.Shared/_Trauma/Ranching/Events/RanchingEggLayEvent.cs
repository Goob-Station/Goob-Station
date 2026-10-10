// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared._Trauma.Ranching.Components;

namespace Content.Goobstation.Shared._Trauma.Ranching.Events;

/// <summary>
/// Raised on the mob and tells it to lay an egg
/// </summary>
/// <param name="Mob">The chicken</param>
[ByRefEvent]
public record struct RanchingEggLayEvent(Entity<RanchingEggLayerComponent> Mob);
