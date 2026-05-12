// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Trauma.Shared.AnimalAgeing.Events;

/// <summary>
/// Raised on the mob and ages it up
/// </summary>
/// <param name="Mob">The mob to age up</param>
/// <param name="Years">How many years should the mob age up</param>
[ByRefEvent]
public record struct AddAgeToMobEvent(Entity<AnimalAgeingComponent> Mob, int Years);
