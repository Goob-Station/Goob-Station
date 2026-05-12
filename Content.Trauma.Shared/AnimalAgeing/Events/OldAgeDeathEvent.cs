// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.AnimalAgeing.Events;

/// <summary>
/// Raise on the mob and kills it
/// </summary>
/// <param name="Mob">The mob raised on</param>
[ByRefEvent]
public record struct OldAgeDeathEvent(Entity<AnimalAgeingComponent> Mob);
