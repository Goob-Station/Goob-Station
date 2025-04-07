// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Body.Components;

/// <summary>
/// Raised when a body gets gibbed, before it is deleted.
/// </summary>
[ByRefEvent]
public readonly record struct BeingGibbedEvent(HashSet<EntityUid> GibbedParts);