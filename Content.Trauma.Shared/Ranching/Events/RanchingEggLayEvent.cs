// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Shared.Ranching.Components;

namespace Content.Trauma.Shared.Ranching.Events;

/// <summary>
/// Raised on the mob and tells it to lay an egg
/// </summary>
/// <param name="Mob">The chicken</param>
[ByRefEvent]
public record struct RanchingEggLayEvent(Entity<RanchingEggLayerComponent> Mob);
