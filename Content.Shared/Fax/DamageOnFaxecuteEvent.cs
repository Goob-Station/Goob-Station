// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS


namespace Content.Shared.Fax.Components;

/// <summary>
/// Event for killing any mob within the fax machine.
/// </summary
[ByRefEvent]
public record struct DamageOnFaxecuteEvent(FaxMachineComponent? Action);

