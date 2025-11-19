// SPDX-FileCopyrightText: 2025 Space Station 14 Contributors
//
// SPDX-License-Identifier: MIT-WIZARDS

namespace Content.Shared.Body.Events;

/// <summary>
/// Raised when a body gets gibbed, before it is deleted.
/// </summary>
[ByRefEvent]
public readonly record struct BeingGibbedEvent(HashSet<EntityUid> GibbedParts);
