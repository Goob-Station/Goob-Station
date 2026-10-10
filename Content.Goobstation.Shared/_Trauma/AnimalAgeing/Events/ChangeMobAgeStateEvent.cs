// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Shared._Trauma.AnimalAgeing.Events;

/// <summary>
/// Raise on the mob when and tells it to change its age state
/// </summary>
/// <param name="Mob">The mob raised on</param>
/// <param name="NewState">The new AnimalAgeState</param>
[ByRefEvent]
public record struct ChangeMobAgeStateEvent(Entity<AnimalAgeingComponent> Mob, AnimalAgeState NewState);
