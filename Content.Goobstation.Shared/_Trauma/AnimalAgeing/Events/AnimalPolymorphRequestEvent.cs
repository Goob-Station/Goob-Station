// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Polymorph;

namespace Content.Goobstation.Shared._Trauma.AnimalAgeing.Events;

/// <summary>
/// Requests that <see cref="Target"/> be polymorphed with the given configuration.
/// </summary>
[ByRefEvent]
public record struct AnimalPolymorphRequestEvent(EntityUid Target, PolymorphConfiguration Configuration);
