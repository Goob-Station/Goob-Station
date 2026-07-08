// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Damage.Events;

/// <summary>
/// Attempting to apply stamina damage on entity.
/// </summary>
[ByRefEvent]
public record struct StaminaDamageOnHitAttemptEvent(bool LightAttack, bool Cancelled); // Goob edit