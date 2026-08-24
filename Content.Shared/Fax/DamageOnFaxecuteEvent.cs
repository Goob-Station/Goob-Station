// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared.Fax.Components;

/// <summary>
/// Event for killing any mob within the fax machine.
/// </summary
[ByRefEvent]
public record struct DamageOnFaxecuteEvent(FaxMachineComponent? Action);
